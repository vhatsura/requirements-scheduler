using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RequirementsScheduler.BLL.Model;
using RequirementsScheduler.BLL.Model.Conflicts;
using RequirementsScheduler.BLL.Service;
using RequirementsScheduler.DAL;
using RequirementsScheduler.DAL.Model;
using RequirementsScheduler.Library.Extensions;
using Experiment = RequirementsScheduler.BLL.Model.Experiment;

namespace RequirementsScheduler.Library.Worker
{
    public sealed class ExperimentPipeline : IExperimentPipeline
    {
        private readonly IOnlineExecutor _onlineExecutor;

        public ExperimentPipeline(
            IExperimentGenerator generator,
            IWorkerExperimentService service,
            IExperimentTestResultService resultService,
            IReportsService reportService,
            ILogger<ExperimentPipeline> logger,
            IOptions<DbSettings> settings, IOnlineExecutor onlineExecutor)
        {
            _onlineExecutor = onlineExecutor;
            Generator = generator ?? throw new ArgumentNullException(nameof(generator));
            Service = service;
            ResultService = resultService;
            ReportService = reportService;
            Logger = logger;
            Settings = settings;
        }

        private IExperimentGenerator Generator { get; }
        private IWorkerExperimentService Service { get; }
        private IExperimentTestResultService ResultService { get; }
        private IReportsService ReportService { get; }
        private ILogger Logger { get; }

        private IOptions<DbSettings> Settings { get; }

        public async Task Run(IEnumerable<Experiment> experiments, bool reportExceptions = true,
            bool stopOnException = false)
        {
            foreach (var experiment in experiments)
            {
                var experimentIdLoggerScope = Logger.BeginScope("{ExperimentId}", experiment.Id);
                Logger.LogInformation("Start to execute experiment");

                experiment.Status = ExperimentStatus.InProgress;
                Service.StartExperiment(experiment.Id);

                try
                {
                    await RunTests(experiment, stopOnException);
                    Logger.LogInformation("Experiment was executed");
                }
                catch (Exception ex)
                {
                    if (!reportExceptions)
                    {
                        throw;
                    }

                    Logger.LogCritical(ex, "Exception occurred during tests run for experiment.");

                    await using var db = new Database(Settings).Open();
                    await db.GetTable<ExperimentFailure>()
                        .InsertAsync(() => new ExperimentFailure
                        {
                            ExperimentId = experiment.Id,
                            ErrorMessage = JsonConvert.SerializeObject(ex)
                        });
                }
                finally
                {
                    experiment.Status = ExperimentStatus.Completed;
                    Service.StopExperiment(experiment.Id);
                    experimentIdLoggerScope?.Dispose();
                }
            }
        }

        private async Task RunTests(Experiment experiment, bool stopOnException = false)
        {
            var experimentReport = new ExperimentReport
            {
                ExperimentId = experiment.Id
            };

            var stop1 = 0;
            var stop2 = 0;
            var stop3 = 0;
            var stop4 = 0;

            var downtimeAmount = 0;

            var sumOfDeltaCmax = 0.0;
            var deltaCmaxMax = 0.0f;

            var offlineResolvedConflictAmount = 0;
            var onlineResolvedConflictAmount = 0;
            var onlineUnResolvedConflictAmount = 0;

            var onlineExecutionTimeInMilliseconds = 0.0d;
            var offlineExecutionTimeInMilliseconds = 0.0d;

            var aggregationResult = new Dictionary<int, ResultInfo>();

            foreach (var testRun in Enumerable.Range(0, experiment.TestsAmount))
            {
                using var _ = Logger.BeginScope("{TestNumber}", testRun + 1);
                Logger.LogInformation("Start to execute test in experiment.");

                var experimentInfo = Generator.GenerateDataForTest(experiment, testRun + 1);

                try
                {
                    RunTest(experimentInfo, ref stop1, ref stop2, ref stop3, ref stop4, ref sumOfDeltaCmax,
                        ref deltaCmaxMax, ref offlineExecutionTimeInMilliseconds, ref onlineExecutionTimeInMilliseconds,
                        ref offlineResolvedConflictAmount,
                        ref onlineResolvedConflictAmount, ref onlineUnResolvedConflictAmount, ref downtimeAmount);
                }
                catch (Exception ex)
                {
                    if (stopOnException)
                    {
                        throw;
                    }

                    Logger.LogCritical(ex,
                        "Exception occurred during test run in scope of experiment. {@ExperimentInfo}", experimentInfo);

                    await using var db = new Database(Settings).Open();
                    await db.GetTable<ExperimentFailure>()
                        .InsertAsync(() => new ExperimentFailure
                        {
                            ExperimentId = experiment.Id,
                            ErrorMessage = JsonConvert.SerializeObject(ex),
                            ExperimentInfo = JsonConvert.SerializeObject(experimentInfo,
                                new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Auto})
                        });
                }

                Logger.LogInformation("Test in experiment was executed.");
                await ResultService.SaveExperimentTestResult(experiment.Id, experimentInfo);

                aggregationResult.Add(experimentInfo.TestNumber, experimentInfo.Result);
            }

            experimentReport.OfflineExecutionTime = TimeSpan.FromMilliseconds(offlineExecutionTimeInMilliseconds);
            experimentReport.OnlineExecutionTime = TimeSpan.FromMilliseconds(onlineExecutionTimeInMilliseconds);

            experimentReport.DeltaCmaxMax = deltaCmaxMax;

            experimentReport.OfflineResolvedConflictAmount = offlineResolvedConflictAmount;
            experimentReport.OnlineResolvedConflictAmount = onlineResolvedConflictAmount;
            experimentReport.OnlineUnResolvedConflictAmount = onlineUnResolvedConflictAmount;

            experimentReport.Stop1Percentage = (float) Math.Round(stop1 / (float) experiment.TestsAmount * 100, 1);
            experimentReport.Stop2Percentage = (float) Math.Round(stop2 / (float) experiment.TestsAmount * 100, 1);
            experimentReport.Stop3Percentage = (float) Math.Round(stop3 / (float) experiment.TestsAmount * 100, 1);
            experimentReport.Stop4Percentage = (float) Math.Round(stop4 / (float) experiment.TestsAmount * 100, 1);

            experimentReport.DowntimeAmount = downtimeAmount;

            if (stop4 != 0)
            {
                experimentReport.DeltaCmaxAverage = (float) sumOfDeltaCmax / experiment.TestsAmount;
            }
            else
            {
                experimentReport.DeltaCmaxAverage = 0;
            }

            await ResultService.SaveAggregatedResult(experiment.Id, aggregationResult);

            ReportService.Save(experimentReport);
        }

        private void RunTest(ExperimentInfo experimentInfo, ref int stop1, ref int stop2, ref int stop3, ref int stop4,
            ref double sumOfDeltaCmax, ref float deltaCmaxMax, ref double offlineExecutionTimeInMilliseconds,
            ref double onlineExecutionTimeInMilliseconds,
            ref int offlineResolvedConflictAmount, ref int onlineResolvedConflictAmount,
            ref int onlineUnResolvedConflictAmount, ref int downtimeAmount)
        {
            var offlineResult = OfflineExecutor.RunInOffline(experimentInfo);

            if (experimentInfo.J12Chain == null || experimentInfo.J12.Any() && !experimentInfo.J12Chain.Any())
            {
                experimentInfo.J12Chain = new Chain(experimentInfo.J12);
                // new Chain(experimentInfo.J12.Select(d => new LaboriousDetail(d.OnFirst, d.OnSecond, d.Number)));
            }


            if (experimentInfo.J21Chain == null || experimentInfo.J21.Any() && !experimentInfo.J21Chain.Any())
            {
                experimentInfo.J21Chain = new Chain(experimentInfo.J21);
                // new Chain(experimentInfo.J21.Select(d => new LaboriousDetail(d.OnFirst, d.OnSecond, d.Number)));
            }

            if (!offlineResult)
            {
                experimentInfo.OnlineChainOnFirstMachine = GetOnlineChainOnFirstMachine(experimentInfo);
                experimentInfo.OnlineChainOnSecondMachine = GetOnlineChainOnSecondMachine(experimentInfo);

                RunOnlineMode(experimentInfo);

                if (!experimentInfo.Result.Online.IsResolvedOnCheck3InOnline)
                {
                    Interlocked.Increment(ref stop2);
                }
                else if (experimentInfo.Result.IsStop3OnOnline)
                {
                    Interlocked.Increment(ref stop3);
                }
                else
                {
                    Interlocked.Increment(ref stop4);
                    InterlockedExtensions.Add(ref sumOfDeltaCmax, experimentInfo.Result.DeltaCmax);
                    InterlockedExtensions.Max(ref deltaCmaxMax, experimentInfo.Result.DeltaCmax);
                }

                Interlocked.Add(ref downtimeAmount,
                    experimentInfo.OnlineChainOnFirstMachine.Count(x => x.Type == OnlineChainType.Downtime) +
                    experimentInfo.OnlineChainOnSecondMachine.Count(x => x.Type == OnlineChainType.Downtime));
            }
            else
            {
                Interlocked.Increment(ref stop1);
            }

            InterlockedExtensions.Add(ref offlineExecutionTimeInMilliseconds,
                experimentInfo.Result.OfflineExecutionTime.TotalMilliseconds);
            InterlockedExtensions.Add(ref onlineExecutionTimeInMilliseconds,
                experimentInfo.Result.Online.ExecutionTime.TotalMilliseconds);

            Interlocked.Add(ref offlineResolvedConflictAmount, experimentInfo.Result.OfflineResolvedConflictAmount);
            Interlocked.Add(ref onlineResolvedConflictAmount, experimentInfo.Result.Online.ResolvedConflictAmount);
            Interlocked.Add(ref onlineUnResolvedConflictAmount,
                experimentInfo.Result.Online.UnresolvedConflictAmount);
        }

        #region Online mode

        private static OnlineChain GetOnlineChainOnFirstMachine(ExperimentInfo experimentInfo)
        {
            var onlineChain = new OnlineChain();
            foreach (var chainNode in experimentInfo.J12Chain)
            {
                var element = chainNode.Type switch
                {
                    ChainType.Detail when chainNode is LaboriousDetail detail => (IOnlineChainNode) detail.OnFirst,
                    ChainType.Conflict when chainNode is Conflict conflict => new OnlineConflict(
                        conflict.Details.Values.Select(d =>
                            new KeyValuePair<int, Detail>(d.OnFirst.Number, d.OnFirst))),
                    _ => throw new InvalidOperationException()
                };

                onlineChain.AddLast(element);
            }

            foreach (var j1 in experimentInfo.J1) onlineChain.AddLast(j1);

            foreach (var chainNode in experimentInfo.J21Chain)
            {
                var element = chainNode.Type switch
                {
                    ChainType.Detail when chainNode is LaboriousDetail detail => (IOnlineChainNode) detail.OnFirst,
                    ChainType.Conflict when chainNode is Conflict conflict => new OnlineConflict(
                        conflict.Details.Values.Select(d =>
                            new KeyValuePair<int, Detail>(d.OnFirst.Number, d.OnFirst))),
                    _ => throw new InvalidOperationException()
                };

                onlineChain.AddLast(element);
            }

            return onlineChain;
        }

        private static OnlineChain GetOnlineChainOnSecondMachine(ExperimentInfo experimentInfo)
        {
            var onlineChain = new OnlineChain();
            foreach (var chainNode in experimentInfo.J21Chain)
            {
                var element = chainNode.Type switch
                {
                    ChainType.Detail when chainNode is LaboriousDetail detail => (IOnlineChainNode) detail.OnSecond,
                    ChainType.Conflict when chainNode is Conflict conflict => new OnlineConflict(
                        conflict.Details.Values.Select(d =>
                            new KeyValuePair<int, Detail>(d.OnSecond.Number, d.OnSecond))),
                    _ => throw new InvalidOperationException()
                };

                onlineChain.AddLast(element);
            }

            foreach (var j2 in experimentInfo.J2) onlineChain.AddLast(j2);

            foreach (var chainNode in experimentInfo.J12Chain)
            {
                var element = chainNode.Type switch
                {
                    ChainType.Detail when chainNode is LaboriousDetail detail => (IOnlineChainNode) detail.OnSecond,
                    ChainType.Conflict when chainNode is Conflict conflict => new OnlineConflict(
                        conflict.Details.Values.Select(d =>
                            new KeyValuePair<int, Detail>(d.OnSecond.Number, d.OnSecond))),
                    _ => throw new InvalidOperationException()
                };

                onlineChain.AddLast(element);
            }

            return onlineChain;
        }

        private void RunOnlineMode(ExperimentInfo experimentInfo)
        {
            Logger.LogInformation("Start to execute online mode.");

            experimentInfo.GenerateP(Generator);

            var processedDetailNumbersOnFirst = experimentInfo.J21.Select(d => d.Number)
                .Union(experimentInfo.J2.Select(d => d.Number)).ToHashSet();

            var processedDetailNumbersOnSecond = experimentInfo.J12.Select(d => d.Number)
                .Union(experimentInfo.J1.Select(d => d.Number)).ToHashSet();

            var onlineContext = _onlineExecutor.Execute(experimentInfo.OnlineChainOnFirstMachine,
                experimentInfo.OnlineChainOnSecondMachine, processedDetailNumbersOnFirst,
                processedDetailNumbersOnSecond);

            var cMax = onlineContext.TimeFromMachinesStart;
            var cOpt = CalculateCOpt(onlineContext.TimeFromMachinesStart, onlineContext.Time2,
                experimentInfo, cMax);

            if (Math.Abs(cOpt - cMax) < 0.0001)
            {
                experimentInfo.Result.IsStop3OnOnline = true;
            }

            experimentInfo.Result.DeltaCmax = (float) ((cMax - cOpt) / cOpt * 100);
            experimentInfo.Result.Online = onlineContext;

            Logger.LogInformation("Online mode was executed.");
        }

        private double CalculateCOpt(double time1, double time2, ExperimentInfo experimentInfo, double cMax)
        {
            double cOpt;
            if (time2 >= time1 && !experimentInfo.OnlineChainOnSecondMachine.Any(node => node is Downtime) ||
                time1 >= time2 && !experimentInfo.OnlineChainOnFirstMachine.Any(node => node is Downtime))
            {
                cOpt = cMax;
            }
            else
            {
                Logger.LogInformation(
                    "Experiment info before run in online mode and after P generation. {@J1}, {@J2}, {@J12}, {@J21}",
                    experimentInfo.J1, experimentInfo.J2, experimentInfo.J12, experimentInfo.J21);

                if (time2 > time1)
                {
                    var j12Numbers = experimentInfo.J12.Select(detail => detail.Number);

                    var j12OnFirstMachine = experimentInfo.OnlineChainOnFirstMachine
                        .OfType<Detail>()
                        .Where(detail => j12Numbers.Contains(detail.Number))
                        .ToList();

                    var j12OnSecondMachine = experimentInfo.OnlineChainOnSecondMachine
                        .OfType<Detail>()
                        .Where(detail => j12Numbers.Contains(detail.Number))
                        .ToList();

                    if (j12OnFirstMachine.Count != j12OnSecondMachine.Count &&
                        !j12OnFirstMachine.Select(detail => detail.Number)
                            .SequenceEqual(j12OnSecondMachine.Select(detail => detail.Number)))
                    {
                        throw new InvalidOperationException("Wrong get J12 from online chains");
                    }

                    var x1 = j12OnFirstMachine
                        .Where(detail =>
                            detail.Time.P <= j12OnSecondMachine.First(d => d.Number == detail.Number).Time.P)
                        .OrderBy(detail => detail.Time.P)
                        .ToList();

                    var x2 = j12OnFirstMachine
                        .Except(x1)
                        .OrderByDescending(detail => j12OnSecondMachine.First(d => d.Number == detail.Number).Time.P)
                        .ToList();

                    j12OnFirstMachine = x1.Concat(x2).ToList();
                    j12OnSecondMachine = j12OnSecondMachine
                        .OrderBy(detail => detail.Number,
                            new CustomComparer(j12OnFirstMachine.Select(detail => detail.Number)))
                        .ToList();

                    var sumOfPOnFirst = j12OnFirstMachine.First().Time.P;
                    var sumOfPOnSecond = j12OnSecondMachine.Sum(detail => detail.Time.P);
                    var maxSumOfP = 0.0;
                    var jOfMaxSumOfP = 0;

                    for (var i = 0; i < j12OnFirstMachine.Count; i++)
                    {
                        var sumOfP = sumOfPOnFirst + sumOfPOnSecond;
                        if (maxSumOfP < sumOfP)
                        {
                            maxSumOfP = sumOfP;
                            jOfMaxSumOfP = i + 1;
                        }

                        if (i != j12OnFirstMachine.Count - 1)
                        {
                            sumOfPOnFirst += j12OnFirstMachine[i + 1].Time.P;
                            sumOfPOnSecond -= j12OnSecondMachine[i].Time.P;
                        }
                    }

                    if (jOfMaxSumOfP == 0)
                    {
                        throw new InvalidOperationException("Wrong finding algorithm of maxCfact");
                    }

                    var l = j12OnFirstMachine
                        .Take(jOfMaxSumOfP)
                        .Sum(detail => detail.Time.P) - j12OnSecondMachine.Take(jOfMaxSumOfP - 1)
                        .Sum(detail => detail.Time.P);
                    var q = experimentInfo.OnlineChainOnSecondMachine
                        .OfType<Detail>()
                        .Where(detail => !j12Numbers.Contains(detail.Number))
                        .Sum(detail => detail.Time.P);

                    if (q >= l)
                    {
                        cOpt = maxSumOfP + (q - l);
                    }
                    else
                    {
                        cOpt = maxSumOfP;
                    }
                }
                else
                {
                    var j21Numbers = experimentInfo.J21.Select(detail => detail.Number);

                    var j21OnFirstMachine = experimentInfo.OnlineChainOnFirstMachine
                        .OfType<Detail>()
                        .Where(detail => j21Numbers.Contains(detail.Number))
                        .ToList();

                    var j21OnSecondMachine = experimentInfo.OnlineChainOnSecondMachine
                        .OfType<Detail>()
                        .Where(detail => j21Numbers.Contains(detail.Number))
                        .ToList();

                    if (j21OnFirstMachine.Count != j21OnSecondMachine.Count &&
                        !j21OnFirstMachine.Select(detail => detail.Number)
                            .SequenceEqual(j21OnSecondMachine.Select(detail => detail.Number)))
                    {
                        throw new InvalidOperationException("Wrong get J21 from online chains");
                    }

                    var x1 = j21OnFirstMachine
                        .Where(detail =>
                            j21OnSecondMachine.First(d => d.Number == detail.Number).Time.P <= detail.Time.P)
                        .OrderBy(detail => j21OnSecondMachine.First(d => d.Number == detail.Number).Time.P)
                        .ToList();

                    var x2 = j21OnFirstMachine
                        .Except(x1)
                        .OrderByDescending(detail => detail.Time.P)
                        .ToList();

                    j21OnFirstMachine = x1.Concat(x2).ToList();
                    j21OnSecondMachine = j21OnSecondMachine
                        .OrderBy(detail => detail.Number,
                            new CustomComparer(j21OnFirstMachine.Select(detail => detail.Number)))
                        .ToList();

                    var sumOfPOnFirst = j21OnFirstMachine.Sum(detail => detail.Time.P);
                    var sumOfPOnSecond = j21OnSecondMachine.First().Time.P;
                    var maxSumOfP = 0.0;
                    var jOfMaxSumOfP = 0;

                    for (var i = 0; i < j21OnFirstMachine.Count; i++)
                    {
                        var sumOfP = sumOfPOnFirst + sumOfPOnSecond;
                        if (maxSumOfP < sumOfP)
                        {
                            maxSumOfP = sumOfP;
                            jOfMaxSumOfP = i + 1;
                        }

                        if (i != j21OnFirstMachine.Count - 1)
                        {
                            sumOfPOnSecond += j21OnSecondMachine[i + 1].Time.P;
                            sumOfPOnFirst -= j21OnFirstMachine[i].Time.P;
                        }
                    }

                    if (jOfMaxSumOfP == 0)
                    {
                        throw new InvalidOperationException("Wrong finding algorithm of maxCfact");
                    }

                    var l = j21OnSecondMachine.Take(jOfMaxSumOfP).Sum(detail => detail.Time.P) -
                            j21OnFirstMachine.Take(jOfMaxSumOfP - 1).Sum(detail => detail.Time.P);
                    var q = experimentInfo.OnlineChainOnFirstMachine
                        .OfType<Detail>()
                        .Where(detail => !j21Numbers.Contains(detail.Number))
                        .Sum(detail => detail.Time.P);

                    if (q >= l)
                    {
                        cOpt = maxSumOfP + (q - l);
                    }
                    else
                    {
                        cOpt = maxSumOfP;
                    }
                }
            }

            return cOpt;
        }

        private class CustomComparer : IComparer<int>
        {
            private readonly List<int> _numbers;

            public CustomComparer(IEnumerable<int> numbers)
            {
                _numbers = numbers.ToList();
            }

            public int Compare(int x, int y) => _numbers.IndexOf(x).CompareTo(_numbers.IndexOf(y));
        }

        #endregion
    }
}
