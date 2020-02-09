using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RequirementsScheduler.BLL.Model;
using RequirementsScheduler.BLL.Service;
using RequirementsScheduler.DAL;
using RequirementsScheduler.DAL.Model;
using RequirementsScheduler.Library.Extensions;
using Experiment = RequirementsScheduler.BLL.Model.Experiment;

namespace RequirementsScheduler.Library.Worker
{
    public sealed class ExperimentPipeline : IExperimentPipeline
    {
        public ExperimentPipeline(
            IExperimentGenerator generator,
            IWorkerExperimentService service,
            IExperimentTestResultService resultService,
            IReportsService reportService,
            ILogger<ExperimentPipeline> logger,
            IOptions<DbSettings> settings)
        {
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

        public async Task Run(IEnumerable<Experiment> experiments)
        {
            foreach (var experiment in experiments)
            {
                experiment.Status = ExperimentStatus.InProgress;
                Service.StartExperiment(experiment.Id);

                try
                {
                    await RunTests(experiment);
                }
                catch (Exception ex)
                {
                    using var db = new Database(Settings).Open();
                    db.GetTable<ExperimentFailure>()
                        .Insert(() => new ExperimentFailure
                        {
                            ExperimentId = experiment.Id,
                            ErrorMessage = JsonConvert.SerializeObject(ex)
                        });
                }
                finally
                {
                    experiment.Status = ExperimentStatus.Completed;
                    Service.StopExperiment(experiment.Id);
                }
            }
        }

        private Task RunTests(Experiment experiment)
        {
            var experimentReport = new ExperimentReport
            {
                ExperimentId = experiment.Id
            };

            var stop1 = 0;
            var stop2 = 0;
            var stop3 = 0;
            var stop4 = 0;

            var sumOfDeltaCmax = 0.0;
            var deltaCmaxMax = 0.0f;

            var offlineResolvedConflictAmount = 0;
            var onlineResolvedConflictAmount = 0;
            var onlineUnResolvedConflictAmount = 0;

            var onlineExecutionTimeInMilliseconds = 0.0d;
            var offlineExecutionTimeInMilliseconds = 0.0d;

            var aggregationResult = Enumerable.Range(0, experiment.TestsAmount)
                //.AsParallel()
                .Select(i =>
                {
                    var experimentInfo = Generator.GenerateDataForTest(experiment, i + 1);

                    RunTest(experimentInfo, ref stop1, ref stop2, ref stop3, ref stop4, ref sumOfDeltaCmax,
                        ref deltaCmaxMax, ref offlineExecutionTimeInMilliseconds, ref onlineExecutionTimeInMilliseconds,
                        ref offlineResolvedConflictAmount,
                        ref onlineResolvedConflictAmount, ref onlineUnResolvedConflictAmount);

                    ResultService.SaveExperimentTestResult(experiment.Id, experimentInfo);

                    return experimentInfo;
                })
                .ToDictionary(x => x.TestNumber, x => x.Result);

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

            if (stop4 != 0)
                experimentReport.DeltaCmaxAverage = (float) sumOfDeltaCmax / experiment.TestsAmount;
            else
                experimentReport.DeltaCmaxAverage = 0;

            ResultService.SaveAggregatedResult(experiment.Id, aggregationResult);

            ReportService.Save(experimentReport);

            return Task.FromResult(0);
        }

        private void RunTest(ExperimentInfo experimentInfo, ref int stop1, ref int stop2, ref int stop3, ref int stop4,
            ref double sumOfDeltaCmax, ref float deltaCmaxMax, ref double offlineExecutionTimeInMilliseconds,
            ref double onlineExecutionTimeInMilliseconds,
            ref int offlineResolvedConflictAmount, ref int onlineResolvedConflictAmount,
            ref int onlineUnResolvedConflictAmount)
        {
            var offlineResult = OfflineExecutor.RunInOffline(experimentInfo);

            if (experimentInfo.J12Chain == null ||
                experimentInfo.J12.Any() && !experimentInfo.J12Chain.Any())
                experimentInfo.J12Chain = new Chain(experimentInfo.J12
                    .Select(d => new LaboriousDetail(d.OnFirst, d.OnSecond, d.Number)));

            if (experimentInfo.J21Chain == null ||
                experimentInfo.J21.Any() && !experimentInfo.J21Chain.Any())
                experimentInfo.J21Chain = new Chain(experimentInfo.J21
                    .Select(d => new LaboriousDetail(d.OnFirst, d.OnSecond, d.Number)));

            if (!offlineResult)
            {
                experimentInfo.OnlineChainOnFirstMachine = GetOnlineChainOnFirstMachine(experimentInfo);
                experimentInfo.OnlineChainOnSecondMachine = GetOnlineChainOnSecondMachine(experimentInfo);

                RunOnlineMode(experimentInfo);

                if (!experimentInfo.Result.IsResolvedOnCheck3InOnline)
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
            }
            else
            {
                Interlocked.Increment(ref stop1);
            }

            InterlockedExtensions.Add(ref offlineExecutionTimeInMilliseconds,
                experimentInfo.Result.OfflineExecutionTime.TotalMilliseconds);
            InterlockedExtensions.Add(ref onlineExecutionTimeInMilliseconds,
                experimentInfo.Result.OnlineExecutionTime.TotalMilliseconds);

            Interlocked.Add(ref offlineResolvedConflictAmount, experimentInfo.Result.OfflineResolvedConflictAmount);
            Interlocked.Add(ref onlineResolvedConflictAmount, experimentInfo.Result.OnlineResolvedConflictAmount);
            Interlocked.Add(ref onlineUnResolvedConflictAmount,
                experimentInfo.Result.OnlineUnResolvedConflictAmount);
        }

        private delegate void ConflictResolverDelegate(OnlineConflict conflict,
            ref LinkedListNode<IOnlineChainNode> node, ResultInfo result, bool isFirst);

        private class CustomComparer : IComparer<int>
        {
            private readonly List<int> _numbers;

            public CustomComparer(IEnumerable<int> numbers)
            {
                _numbers = numbers.ToList();
            }

            public int Compare(int x, int y) => _numbers.IndexOf(x).CompareTo(_numbers.IndexOf(y));
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

        private static void RunOnlineMode(ExperimentInfo experimentInfo)
        {
            experimentInfo.OnlineChainOnFirstMachine.GenerateP();
            experimentInfo.OnlineChainOnSecondMachine.GenerateP();

            DoRunInOnlineMode(experimentInfo);
        }

        private static void ProcessDetailOnMachine(
            ref IOnlineChainNode currentDetail,
            ISet<int> processedDetailNumbersOnCurrentMachine,
            ISet<int> processedDetailNumbersOnAnotherMachine,
            ref LinkedListNode<IOnlineChainNode> nodeOnCurrentMachine,
            ref LinkedListNode<IOnlineChainNode> nodeOnAnotherMachine,
            double timeFromMachinesStart,
            OnlineChain chainOnCurrentMachine,
            OnlineChain chainOnAnotherMachine,
            ref double timeOnCurrentMachine,
            out bool hasDetailOnCurrentMachine,
            bool isFirstDetail,
            ResultInfo result)
        {
            if (currentDetail is Detail detail1) processedDetailNumbersOnCurrentMachine.Add(detail1.Number);

            currentDetail = nodeOnCurrentMachine.Value;

            var anotherMachine = nodeOnAnotherMachine;
            var machinesStart = timeFromMachinesStart;
            ProcessDetailOnMachine(
                chainOnCurrentMachine,
                ref nodeOnCurrentMachine,
                detail => chainOnAnotherMachine
                              .TakeWhile(d => (d as Detail).Number != detail.Number)
                              .Sum(d => (d as Detail).Time.P) +
                          (chainOnAnotherMachine.First(
                              d => d is Detail && (d as Detail).Number == detail.Number) as Detail).Time.P -
                          machinesStart,
                processedDetailNumbersOnAnotherMachine,
                ref timeOnCurrentMachine,
                isFirstDetail,
                result,
                (OnlineConflict conflict, ref LinkedListNode<IOnlineChainNode> node, ResultInfo resultInfo,
                    bool isFirst) =>
                {
                    ResolveConflictOnMachine(conflict, ref node, ref anotherMachine, isFirst, chainOnCurrentMachine,
                        chainOnAnotherMachine, machinesStart, resultInfo);
                });

            nodeOnAnotherMachine = anotherMachine;

            if (currentDetail is OnlineConflict) currentDetail = nodeOnCurrentMachine.Value;

            if (nodeOnCurrentMachine.Value is Downtime) currentDetail = null;

            nodeOnCurrentMachine = nodeOnCurrentMachine.Next;
            hasDetailOnCurrentMachine = nodeOnCurrentMachine != null;
        }

        private static void DoRunInOnlineMode(ExperimentInfo experimentInfo)
        {
            var processedDetailNumbersOnFirst = experimentInfo.J21.Select(d => d.Number)
                .Union(experimentInfo.J2.Select(d => d.Number)).ToHashSet();

            var processedDetailNumbersOnSecond = experimentInfo.J12.Select(d => d.Number)
                .Union(experimentInfo.J1.Select(d => d.Number)).ToHashSet();

            IOnlineChainNode currentDetailOnFirst = null;
            IOnlineChainNode currentDetailOnSecond = null;

            var timeFromMachinesStart = 0.0;
            var time1 = 0.0;
            var time2 = 0.0;

            var isFirstDetail = true;

            var nodeOnFirstMachine = experimentInfo.OnlineChainOnFirstMachine.First;
            var nodeOnSecondMachine = experimentInfo.OnlineChainOnSecondMachine.First;

            var hasDetailOnFirst = nodeOnFirstMachine != null;
            var hasDetailOnSecond = nodeOnSecondMachine != null;

            // details are on two machines
            while (hasDetailOnFirst && hasDetailOnSecond ||
                   time1 > time2 && hasDetailOnSecond ||
                   time2 > time1 && hasDetailOnFirst)
            {
                // time1 equal to time2
                if (Math.Abs(time1 - time2) < 0.001)
                {
                    ProcessDetailOnMachine(
                        ref currentDetailOnFirst,
                        processedDetailNumbersOnFirst,
                        processedDetailNumbersOnSecond,
                        ref nodeOnFirstMachine,
                        ref nodeOnSecondMachine,
                        timeFromMachinesStart,
                        experimentInfo.OnlineChainOnFirstMachine,
                        experimentInfo.OnlineChainOnSecondMachine,
                        ref time1,
                        out hasDetailOnFirst,
                        isFirstDetail,
                        experimentInfo.Result);

                    //if (nodeOnSecondMachine?.List == null)
                    //{
                    //    if (isFirstDetail)
                    //        nodeOnSecondMachine = experimentInfo.OnlineChainOnSecondMachine.First;
                    //}

                    ProcessDetailOnMachine(
                        ref currentDetailOnSecond,
                        processedDetailNumbersOnSecond,
                        processedDetailNumbersOnFirst,
                        ref nodeOnSecondMachine,
                        ref nodeOnFirstMachine,
                        timeFromMachinesStart,
                        experimentInfo.OnlineChainOnSecondMachine,
                        experimentInfo.OnlineChainOnFirstMachine,
                        ref time2,
                        out hasDetailOnSecond,
                        isFirstDetail,
                        experimentInfo.Result);
                }
                else if (time1 < time2)
                {
                    ProcessDetailOnMachine(
                        ref currentDetailOnFirst,
                        processedDetailNumbersOnFirst,
                        processedDetailNumbersOnSecond,
                        ref nodeOnFirstMachine,
                        ref nodeOnSecondMachine,
                        timeFromMachinesStart,
                        experimentInfo.OnlineChainOnFirstMachine,
                        experimentInfo.OnlineChainOnSecondMachine,
                        ref time1,
                        out hasDetailOnFirst,
                        isFirstDetail,
                        experimentInfo.Result);
                }
                else
                {
                    ProcessDetailOnMachine(
                        ref currentDetailOnSecond,
                        processedDetailNumbersOnSecond,
                        processedDetailNumbersOnFirst,
                        ref nodeOnSecondMachine,
                        ref nodeOnFirstMachine,
                        timeFromMachinesStart,
                        experimentInfo.OnlineChainOnSecondMachine,
                        experimentInfo.OnlineChainOnFirstMachine,
                        ref time2,
                        out hasDetailOnSecond,
                        isFirstDetail,
                        experimentInfo.Result);
                }

                timeFromMachinesStart = Math.Min(time1, time2);

                isFirstDetail = false;
            }

            // details only on first machines
            if (hasDetailOnFirst)
                for (var node = nodeOnFirstMachine; node != null; node = node.Next)
                    if (node.Value is Detail detail)
                        time1 += detail.Time.P;
                    else
                        throw new InvalidOperationException(
                            "There can be no conflicts and downtimes when one of machine finished work");
            else
                // details only on second machine
                for (var node = nodeOnSecondMachine; node != null; node = node.Next)
                    if (node.Value is Detail detail)
                        time2 += detail.Time.P;
                    else
                        throw new InvalidOperationException(
                            "There can be no conflicts and downtimes when one of machine finished work");

            timeFromMachinesStart = Math.Max(time1, time2);

            var cMax = timeFromMachinesStart;
            var cOpt = CalculateCOpt(time1, time2, experimentInfo, cMax);

            if (Math.Abs(cOpt - cMax) < 0.0001) experimentInfo.Result.IsStop3OnOnline = true;

            experimentInfo.Result.DeltaCmax = (float) ((cMax - cOpt) / cOpt * 100);
        }

        private static double CalculateCOpt(double time1, double time2, ExperimentInfo experimentInfo, double cMax)
        {
            double cOpt;
            if (time2 >= time1 && !experimentInfo.OnlineChainOnSecondMachine.Any(node => node is Downtime) ||
                time1 >= time2 && !experimentInfo.OnlineChainOnFirstMachine.Any(node => node is Downtime))
            {
                cOpt = cMax;
            }
            else
            {
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
                        throw new InvalidOperationException("Wrong get J12 from online chains");

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
                        throw new InvalidOperationException("Wrong finding algorithm of maxCfact");

                    var l = j12OnFirstMachine
                                .Take(jOfMaxSumOfP)
                                .Sum(detail => detail.Time.P) - j12OnSecondMachine.Take(jOfMaxSumOfP - 1)
                                .Sum(detail => detail.Time.P);
                    var q = experimentInfo.OnlineChainOnSecondMachine
                        .OfType<Detail>()
                        .Where(detail => !j12Numbers.Contains(detail.Number))
                        .Sum(detail => detail.Time.P);

                    if (q >= l)
                        cOpt = maxSumOfP + (q - l);
                    else
                        cOpt = maxSumOfP;
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
                        throw new InvalidOperationException("Wrong get J21 from online chains");

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
                        throw new InvalidOperationException("Wrong finding algorithm of maxCfact");

                    var l = j21OnSecondMachine.Take(jOfMaxSumOfP).Sum(detail => detail.Time.P) -
                            j21OnFirstMachine.Take(jOfMaxSumOfP - 1).Sum(detail => detail.Time.P);
                    var q = experimentInfo.OnlineChainOnFirstMachine
                        .OfType<Detail>()
                        .Where(detail => !j21Numbers.Contains(detail.Number))
                        .Sum(detail => detail.Time.P);

                    if (q >= l)
                        cOpt = maxSumOfP + (q - l);
                    else
                        cOpt = maxSumOfP;
                }
            }

            return cOpt;
        }

        private static void ProcessDetailOnMachine(
            OnlineChain chain,
            ref LinkedListNode<IOnlineChainNode> node,
            Func<Detail, double> downtimeCalculationFunc,
            ISet<int> processedDetailNumbersOnAnotherMachine,
            ref double time,
            bool isFirstDetail,
            ResultInfo result,
            ConflictResolverDelegate conflictResolver)
        {
            var currentDetail = node.Value;

            if (currentDetail.Type == OnlineChainType.Conflict)
            {
                var conflict = currentDetail as OnlineConflict;

                var start = Stopwatch.GetTimestamp();
                conflictResolver(conflict, ref node, result, isFirstDetail);
                var stop = Stopwatch.GetTimestamp();

                result.OnlineExecutionTime = result.OnlineExecutionTime.Add(
                    TimeSpan.FromMilliseconds((stop - start) / (double) Stopwatch.Frequency * 1000));


                if (node.Value.Type != OnlineChainType.Detail)
                    throw new InvalidOperationException("Conflict resolver didn't change current node");

                currentDetail = node.Value;
            }

            if (!(currentDetail is Detail detail))
                throw new InvalidCastException(
                    $"Try cast {currentDetail.GetType().FullName} to {typeof(Detail).FullName}");

            if (processedDetailNumbersOnAnotherMachine.Contains(detail.Number))
            {
                time += detail.Time.P;
            }
            else
            {
                var downTime = downtimeCalculationFunc(detail);

                if (Math.Abs(downTime) < 0.000000000000001)
                {
                    node = node.Previous;
                    return;
                }

                node = chain.AddBefore(node, new Downtime(downTime));

                time += downTime;
            }
        }

        private static void ResolveConflictOnMachine(
            OnlineConflict conflict,
            ref LinkedListNode<IOnlineChainNode> nodeOnCurrentMachine,
            ref LinkedListNode<IOnlineChainNode> nodeOnAnotherMachine,
            bool isFirstDetail,
            OnlineChain chainOnCurrentMachine,
            OnlineChain chainOnAnotherMachine,
            double timeFromMachinesStart,
            ResultInfo result)
        {
            if (!(chainOnAnotherMachine.First(i =>
                    i.Type == OnlineChainType.Conflict &&
                    (i as OnlineConflict).Details.Keys.SequenceEqual(conflict.Details.Keys)) is OnlineConflict
                conflictOnAnotherMachine))
                throw new InvalidOperationException("Not found conflict on another machine");

            var conflictNodeOnAnotherMachine = chainOnAnotherMachine.Find(conflictOnAnotherMachine);
            var nodeOnCurrentMachineToRemove = nodeOnCurrentMachine;

            if (!isFirstDetail)
            {
                var sumOfPOnCurrent = chainOnCurrentMachine
                    .TakeWhile(i => !Equals(i, nodeOnCurrentMachineToRemove.Value))
                    .Sum(i => i.Type switch
                    {
                        OnlineChainType.Detail when i is Detail detail => detail.Time.P,
                        OnlineChainType.Downtime when i is Downtime downtime => downtime.Time,
                        _ => throw new InvalidOperationException()
                    });

                var sumOfBInConflictOnCurrent = conflict.Details.Values.Sum(d => d.Time.B);

                var localNodeOnAnotherMachine = nodeOnAnotherMachine;

                var sumOfPOnAnother = chainOnAnotherMachine
                    .TakeWhile(i => !Equals(i, localNodeOnAnotherMachine.Value))
                    .Sum(i => i.Type switch
                    {
                        OnlineChainType.Detail when i is Detail detail => detail.Time.P,
                        OnlineChainType.Downtime when i is Downtime downtime => downtime.Time,
                        _ => throw new InvalidOperationException()
                    });

                var l = timeFromMachinesStart - sumOfPOnAnother;
                var sumOnAnother = l <= (nodeOnAnotherMachine.Value as Detail).Time.A
                    ? (nodeOnAnotherMachine.Value as Detail).Time.A
                    : l;

                var sumOfAOnAnother = chainOnAnotherMachine
                    .SkipWhile(i => !Equals(i, localNodeOnAnotherMachine.Value))
                    .Skip(1)
                    .TakeWhile(i => !Equals(i, conflictOnAnotherMachine))
                    .Sum(i => i.Type switch
                    {
                        OnlineChainType.Detail when i is Detail detail => detail.Time.A,
                        OnlineChainType.Conflict when i is OnlineConflict onlineConflict => onlineConflict
                            .Details.Values.Sum(d => d.Time.A),
                        _ => throw new InvalidOperationException()
                    });

                var sumOnMCurrent = sumOfPOnCurrent + sumOfBInConflictOnCurrent;
                var sumOnMAnother = sumOfPOnAnother + sumOnAnother + sumOfAOnAnother;

                // Check 1
                if (sumOnMCurrent <= sumOnMAnother)
                {
                    nodeOnCurrentMachine = nodeOnCurrentMachineToRemove.Previous;

                    foreach (var conflictDetail in conflict.Details.Values)
                        chainOnCurrentMachine.AddBefore(nodeOnCurrentMachineToRemove, conflictDetail);

                    chainOnCurrentMachine.Remove(nodeOnCurrentMachineToRemove);

                    foreach (var conflictDetail in conflictOnAnotherMachine.Details.Values)
                        chainOnAnotherMachine.AddBefore(conflictNodeOnAnotherMachine, conflictDetail);

                    chainOnAnotherMachine.Remove(conflictNodeOnAnotherMachine);

                    nodeOnCurrentMachine = nodeOnCurrentMachine.Next;

                    result.OnlineResolvedConflictAmount += 1;

                    return;
                }

                // Check 2
                var x1 = conflict.Details.Values
                    .Where(d => conflictOnAnotherMachine.Details[d.Number].Time.A - d.Time.B >= 0)
                    .OrderBy(d => d.Time.B)
                    .ToList();

                var x2 = conflict.Details.Values
                    .Except(x1)
                    .OrderByDescending(d => conflictOnAnotherMachine.Details[d.Number].Time.A)
                    .ToList();

                var conflictSequence = x1.Concat(x2).ToList();

                var firstSum = sumOfPOnCurrent;
                var secondSum = sumOnMAnother;
                var isOptimized = true;

                foreach (var detail in conflictSequence)
                    //todo check all conditions
                    if (firstSum + detail.Time.B <= secondSum)
                    {
                        firstSum += detail.Time.B;
                        secondSum += conflictOnAnotherMachine.Details[detail.Number].Time.A;
                    }
                    else
                    {
                        isOptimized = false;
                        break;
                    }

                if (isOptimized)
                {
                    nodeOnCurrentMachine = nodeOnCurrentMachineToRemove.Previous;

                    foreach (var detail in conflictSequence)
                    {
                        chainOnCurrentMachine.AddBefore(nodeOnCurrentMachineToRemove, detail);
                        chainOnAnotherMachine.AddBefore(conflictNodeOnAnotherMachine,
                            conflictOnAnotherMachine.Details[detail.Number]);
                    }

                    chainOnCurrentMachine.Remove(nodeOnCurrentMachineToRemove);
                    chainOnAnotherMachine.Remove(conflictNodeOnAnotherMachine);

                    nodeOnCurrentMachine = nodeOnCurrentMachine.Next;

                    result.OnlineResolvedConflictAmount += 1;

                    return;
                }

                ResolveConflictOnCheck3(
                    conflict,
                    conflictOnAnotherMachine,
                    ref nodeOnCurrentMachine,
                    ref nodeOnAnotherMachine,
                    nodeOnCurrentMachineToRemove,
                    conflictNodeOnAnotherMachine,
                    chainOnCurrentMachine,
                    chainOnAnotherMachine,
                    result);

                return;
            }

            ResolveConflictOnCheck3(
                conflict,
                conflictOnAnotherMachine,
                ref nodeOnCurrentMachine,
                ref nodeOnAnotherMachine,
                nodeOnCurrentMachineToRemove,
                conflictNodeOnAnotherMachine,
                chainOnCurrentMachine,
                chainOnAnotherMachine,
                result);
        }

        private static void ResolveConflictOnCheck3(
            OnlineConflict conflict,
            OnlineConflict conflictOnAnotherMachine,
            ref LinkedListNode<IOnlineChainNode> nodeOnCurrentMachine,
            ref LinkedListNode<IOnlineChainNode> nodeOnAnotherMachine,
            LinkedListNode<IOnlineChainNode> nodeOnCurrentMachineToRemove,
            LinkedListNode<IOnlineChainNode> conflictNodeOnAnotherMachine,
            OnlineChain chainOnCurrentMachine,
            OnlineChain chainOnAnotherMachine,
            ResultInfo result)
        {
            // Check 3
            var t1 = conflict.Details.Values
                .Where(d => d.Time.Average <= conflictOnAnotherMachine.Details[d.Number].Time.Average)
                .OrderBy(d => d.Time.Average)
                .ToList();

            var t2 = conflict.Details.Values
                .Except(t1)
                .OrderByDescending(d => conflictOnAnotherMachine.Details[d.Number].Time.Average)
                .ToList();

            var conflictSequence = t1.Concat(t2);

            var isFirstAdd = true;
            foreach (var detail in conflictSequence)
                if (isFirstAdd)
                {
                    nodeOnCurrentMachine = chainOnCurrentMachine.AddBefore(nodeOnCurrentMachineToRemove, detail);
                    if (nodeOnAnotherMachine.Value.Type == OnlineChainType.Conflict)
                        nodeOnAnotherMachine = chainOnAnotherMachine.AddBefore(conflictNodeOnAnotherMachine,
                            conflictOnAnotherMachine.Details[detail.Number]);
                    else
                        chainOnAnotherMachine.AddBefore(conflictNodeOnAnotherMachine,
                            conflictOnAnotherMachine.Details[detail.Number]);

                    isFirstAdd = false;
                }
                else
                {
                    chainOnCurrentMachine.AddBefore(nodeOnCurrentMachineToRemove, detail);
                    chainOnAnotherMachine.AddBefore(conflictNodeOnAnotherMachine,
                        conflictOnAnotherMachine.Details[detail.Number]);
                }

            chainOnCurrentMachine.Remove(nodeOnCurrentMachineToRemove);
            chainOnAnotherMachine.Remove(conflictNodeOnAnotherMachine);

            result.OnlineUnResolvedConflictAmount += 1;
            result.IsResolvedOnCheck3InOnline = true;
        }

        #endregion
    }
}