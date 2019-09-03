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
using RequirementsScheduler.BLL;
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
                    using (var db = new Database(Settings).Open())
                    {
                        db.GetTable<ExperimentFailure>()
                            .Insert(() => new ExperimentFailure
                            {
                                ExperimentId = experiment.Id,
                                ErrorMessage = JsonConvert.SerializeObject(ex)
                            });
                    }
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

            var aggregationResult = Enumerable.Range(0, experiment.TestsAmount)
                //.AsParallel()
                .Select(i =>
                {
                    var experimentInfo = Generator.GenerateDataForTest(experiment, i + 1);

                    RunTest(experimentInfo, ref stop1, ref stop2, ref stop3, ref stop4, ref sumOfDeltaCmax,
                        ref deltaCmaxMax, ref onlineExecutionTimeInMilliseconds, ref offlineResolvedConflictAmount,
                        ref onlineResolvedConflictAmount, ref onlineUnResolvedConflictAmount);

                    ResultService.SaveExperimentTestResult(experiment.Id, experimentInfo);

                    return experimentInfo;
                })
                .ToDictionary(x => x.TestNumber, x => x.Result);

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
            ref double sumOfDeltaCmax, ref float deltaCmaxMax, ref double onlineExecutionTimeInMilliseconds,
            ref int offlineResolvedConflictAmount, ref int onlineResolvedConflictAmount,
            ref int onlineUnResolvedConflictAmount)
        {
            var offlineResult = CheckOffline(experimentInfo);

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

            InterlockedExtensions.Add(ref onlineExecutionTimeInMilliseconds,
                experimentInfo.Result.OnlineExecutionTime.TotalMilliseconds);

            Interlocked.Add(ref offlineResolvedConflictAmount, experimentInfo.Result.OfflineResolvedConflictAmount);
            Interlocked.Add(ref onlineResolvedConflictAmount, experimentInfo.Result.OnlineResolvedConflictAmount);
            Interlocked.Add(ref onlineUnResolvedConflictAmount,
                experimentInfo.Result.OnlineUnResolvedConflictAmount);
        }

        private delegate void ConflictResolverDelegate(OnlineConflict conflict,
            ref LinkedListNode<IOnlineChainNode> node, bool isFirst);

        private class CustomComparer : IComparer<int>
        {
            private readonly List<int> _numbers;

            public CustomComparer(IEnumerable<int> numbers)
            {
                _numbers = numbers.ToList();
            }

            public int Compare(int x, int y)
            {
                return _numbers.IndexOf(x).CompareTo(_numbers.IndexOf(y));
            }
        }

        #region Offline mode

        private static bool CheckOffline(ExperimentInfo experimentInfo)
        {
            if (CheckStopOneAndOne(experimentInfo))
            {
                experimentInfo.Result.Type = ResultType.STOP1_1;
                return true;
            }

            if (CheckStopOneAndTwo(experimentInfo))
            {
                experimentInfo.Result.Type = ResultType.STOP1_2;
                return true;
            }

            var stop12ConflictCount = experimentInfo.OfflineConflictCount;

            if (CheckStopOneAndThree(experimentInfo))
            {
                experimentInfo.Result.Type = ResultType.STOP1_3;
                experimentInfo.Result.OfflineResolvedConflictAmount += stop12ConflictCount;
                return true;
            }

            if (CheckStopOneAndFour(experimentInfo))
            {
                experimentInfo.Result.Type = ResultType.STOP1_4;
                experimentInfo.Result.OfflineResolvedConflictAmount += stop12ConflictCount;
                return true;
            }

            experimentInfo.Result.OfflineResolvedConflictAmount +=
                stop12ConflictCount - experimentInfo.OfflineConflictCount;
            return false;
        }

        private static bool CheckStopOneAndOne(ExperimentInfo experimentInfo)
        {
            CheckFirst(experimentInfo);

            if (experimentInfo.IsOptimized) return true;

            CheckSecond(experimentInfo);
            return experimentInfo.IsOptimized;
        }

        private static bool CheckStopOneAndTwo(ExperimentInfo experimentInfo)
        {
            if (!experimentInfo.J12.IsOptimized) experimentInfo.J12Chain = TryToOptimizeJ12(experimentInfo);

            if (!experimentInfo.J21.IsOptimized) experimentInfo.J21Chain = TryToOptimizeJ21(experimentInfo);

            return experimentInfo.IsOptimized;
        }

        private static bool CheckStopOneAndThree(ExperimentInfo experimentInfo)
        {
            if (experimentInfo.J12Chain != null && !experimentInfo.J12Chain.IsOptimized)
                for (var node = experimentInfo.J12Chain.First; node != null; node = node.Next)
                {
                    if (node.Value.Type != ChainType.Conflict) continue;

                    var sumOfBOnFirst = 0.0;
                    var sumOfAOnSecond = 0.0;
                    for (var nodeForSum = experimentInfo.J12Chain.First;
                        nodeForSum != node;
                        nodeForSum = nodeForSum.Next)
                        if (nodeForSum.Value.Type == ChainType.Conflict)
                        {
                            sumOfBOnFirst +=
                                (nodeForSum.Value as Conflict).DetailsDictionary.Values.Sum(detail =>
                                    detail.OnFirst.Time.B);
                            sumOfAOnSecond +=
                                (nodeForSum.Value as Conflict).DetailsDictionary.Values.Sum(detail =>
                                    detail.OnSecond.Time.A);
                        }
                        else
                        {
                            sumOfBOnFirst += (nodeForSum.Value as LaboriousDetail).OnFirst.Time.B;
                            sumOfAOnSecond += (nodeForSum.Value as LaboriousDetail).OnSecond.Time.A;
                        }

                    sumOfBOnFirst +=
                        (node.Value as Conflict).DetailsDictionary.Values.Sum(detail => detail.OnFirst.Time.B);

                    sumOfAOnSecond += experimentInfo.J21.Sum(detail => detail.OnSecond.Time.A) +
                                      experimentInfo.J2.Sum(detail => detail.Time.A);

                    if (sumOfAOnSecond >= sumOfBOnFirst)
                    {
                        var insertedNode = node;
                        foreach (var laboriousDetail in (node.Value as Conflict).DetailsDictionary.Values)
                            insertedNode = experimentInfo.J12Chain.AddBefore(node, laboriousDetail);

                        experimentInfo.J12Chain.Remove(node);
                        node = insertedNode;
                    }
                }

            if (experimentInfo.J21Chain != null && !experimentInfo.J21Chain.IsOptimized)
                for (var node = experimentInfo.J21Chain.First; node != null; node = node.Next)
                {
                    if (node.Value.Type != ChainType.Conflict) continue;

                    var sumOfBOnSecond = 0.0;
                    var sumOfAOnFirst = 0.0;
                    for (var nodeForSum = experimentInfo.J21Chain.First;
                        nodeForSum != node;
                        nodeForSum = nodeForSum.Next)
                        if (nodeForSum.Value.Type == ChainType.Conflict)
                        {
                            sumOfBOnSecond +=
                                (nodeForSum.Value as Conflict).DetailsDictionary.Values.Sum(detail =>
                                    detail.OnSecond.Time.B);
                            sumOfAOnFirst +=
                                (nodeForSum.Value as Conflict).DetailsDictionary.Values.Sum(detail =>
                                    detail.OnFirst.Time.A);
                        }
                        else
                        {
                            sumOfBOnSecond += (nodeForSum.Value as LaboriousDetail).OnSecond.Time.B;
                            sumOfAOnFirst += (nodeForSum.Value as LaboriousDetail).OnFirst.Time.A;
                        }

                    sumOfBOnSecond +=
                        (node.Value as Conflict).DetailsDictionary.Values.Sum(detail => detail.OnSecond.Time.B);

                    sumOfAOnFirst += experimentInfo.J12.Sum(detail => detail.OnFirst.Time.A) +
                                     experimentInfo.J1.Sum(detail => detail.Time.A);

                    if (sumOfAOnFirst >= sumOfBOnSecond)
                    {
                        var insertedNode = node;
                        foreach (var laboriousDetail in (node.Value as Conflict).DetailsDictionary.Values)
                            insertedNode = experimentInfo.J21Chain.AddBefore(node, laboriousDetail);

                        experimentInfo.J21Chain.Remove(node);
                        node = insertedNode;
                    }
                }

            return experimentInfo.IsOptimized;
        }

        private static bool CheckStopOneAndFour(ExperimentInfo experimentInfo)
        {
            if (experimentInfo.J12Chain != null && !experimentInfo.J12Chain.IsOptimized)
                for (var node = experimentInfo.J12Chain.Last; node != null; node = node.Previous)
                {
                    if (node.Value.Type != ChainType.Conflict) continue;

                    var conflict = node.Value as Conflict;
                    var x1Box = conflict.DetailsDictionary.Values
                        .Where(detail => detail.OnSecond.Time.A - detail.OnFirst.Time.B >= 0)
                        .OrderBy(detail => detail.OnFirst.Time.B)
                        .ToList();

                    var x2Box = conflict.DetailsDictionary.Values
                        .Except(x1Box)
                        .OrderBy(detail => detail.OnSecond.Time.A)
                        .Reverse()
                        .ToList();

                    var xBox = x1Box.Concat(x2Box).ToList();

                    var sumBeforeConflictOfBOnFirst = 0.0;
                    var sumBeforeConflictOfAOnSecond = 0.0;

                    for (var nodeForSum = experimentInfo.J12Chain.First;
                        nodeForSum != node;
                        nodeForSum = nodeForSum.Next)
                        if (nodeForSum.Value.Type == ChainType.Conflict)
                        {
                            sumBeforeConflictOfBOnFirst +=
                                (nodeForSum.Value as Conflict).DetailsDictionary.Values.Sum(detail =>
                                    detail.OnFirst.Time.B);
                            sumBeforeConflictOfAOnSecond +=
                                (nodeForSum.Value as Conflict).DetailsDictionary.Values.Sum(detail =>
                                    detail.OnSecond.Time.A);
                        }
                        else
                        {
                            sumBeforeConflictOfBOnFirst += (nodeForSum.Value as LaboriousDetail).OnFirst.Time.B;
                            sumBeforeConflictOfAOnSecond += (nodeForSum.Value as LaboriousDetail).OnSecond.Time.A;
                        }

                    sumBeforeConflictOfAOnSecond += experimentInfo.J21.Sum(detail => detail.OnSecond.Time.A) +
                                                    experimentInfo.J2.Sum(detail => detail.Time.A);

                    var sumOfBOnFirst = sumBeforeConflictOfBOnFirst;
                    var sumOfAOnSecond = sumBeforeConflictOfAOnSecond;

                    var isResolved = true;

                    foreach (var detailInBox in xBox)
                    {
                        sumOfBOnFirst += detailInBox.OnFirst.Time.B;
                        if (sumOfBOnFirst <= sumOfAOnSecond)
                        {
                            sumOfAOnSecond += detailInBox.OnSecond.Time.A;
                            continue;
                        }

                        isResolved = false;
                        break;
                    }

                    if (isResolved)
                    {
                        var insertedNode = node;
                        foreach (var detailInBox in xBox)
                            insertedNode = experimentInfo.J12Chain.AddBefore(node, detailInBox);

                        experimentInfo.J12Chain.Remove(node);
                        node = insertedNode;
                        continue;
                    }

                    var y1Box = conflict.DetailsDictionary.Values
                        .Where(detail => detail.OnFirst.Time.A - detail.OnSecond.Time.B >= 0)
                        .OrderBy(detail => detail.OnSecond.Time.B)
                        .Reverse()
                        .ToList();

                    var y2Box = conflict.DetailsDictionary.Values
                        .Except(y1Box)
                        .OrderBy(detail => detail.OnFirst.Time.A)
                        .ToList();

                    var yBox = y2Box.Concat(y1Box).ToList();

                    double aOfDetailAfterConflict;

                    if (node.Next == null)
                    {
                        aOfDetailAfterConflict = experimentInfo.J1?.Sum(detail => detail.Time.A) ??
                                                 0.0 + experimentInfo.J21?.Sum(detail => detail.OnFirst.Time.A) ?? 0.0;
                        if (aOfDetailAfterConflict == 0.0) break;
                    }
                    else
                    {
                        aOfDetailAfterConflict = node.Next.Value.Type == ChainType.Detail
                            ? (node.Next.Value as LaboriousDetail).OnFirst.Time.A
                            : (node.Next.Value as Conflict).DetailsDictionary.Values.Min(
                                detail => detail.OnFirst.Time.A);
                    }

                    var sumOfAOnFirst = aOfDetailAfterConflict;
                    var sumOfBOnSecond = 0.0;
                    isResolved = true;
                    foreach (var detailInBox in yBox.AsEnumerable().Reverse())
                    {
                        sumOfBOnSecond += detailInBox.OnSecond.Time.B;
                        if (sumOfAOnFirst >= sumOfBOnSecond)
                        {
                            sumOfAOnFirst += detailInBox.OnFirst.Time.A;
                            continue;
                        }

                        isResolved = false;
                        break;
                    }

                    if (isResolved)
                    {
                        var insertedNode = node;
                        foreach (var detailInBox in yBox)
                            insertedNode = experimentInfo.J12Chain.AddBefore(node, detailInBox);

                        experimentInfo.J12Chain.Remove(node);
                        node = insertedNode;
                    }
                }

            if (experimentInfo.J21Chain != null && !experimentInfo.J21Chain.IsOptimized)
                for (var node = experimentInfo.J21Chain.Last; node != null; node = node.Previous)
                {
                    if (node.Value.Type != ChainType.Conflict) continue;

                    var conflict = node.Value as Conflict;
                    var x1Box = conflict.DetailsDictionary.Values
                        .Where(detail => detail.OnFirst.Time.A - detail.OnSecond.Time.B >= 0)
                        .OrderBy(detail => detail.OnSecond.Time.B)
                        .ToList();

                    var x2Box = conflict.DetailsDictionary.Values
                        .Except(x1Box)
                        .OrderBy(detail => detail.OnFirst.Time.A)
                        .Reverse()
                        .ToList();

                    var xBox = x1Box.Concat(x2Box).ToList();

                    var sumBeforeConflictOfBOnSecond = 0.0;
                    var sumBeforeConflictOfAOnFirst = 0.0;

                    for (var nodeForSum = experimentInfo.J21Chain.First;
                        nodeForSum != node;
                        nodeForSum = nodeForSum.Next)
                        if (nodeForSum.Value.Type == ChainType.Conflict)
                        {
                            sumBeforeConflictOfBOnSecond +=
                                (nodeForSum.Value as Conflict).DetailsDictionary.Values.Sum(detail =>
                                    detail.OnSecond.Time.B);
                            sumBeforeConflictOfAOnFirst +=
                                (nodeForSum.Value as Conflict).DetailsDictionary.Values.Sum(detail =>
                                    detail.OnFirst.Time.A);
                        }
                        else
                        {
                            sumBeforeConflictOfBOnSecond += (nodeForSum.Value as LaboriousDetail).OnSecond.Time.B;
                            sumBeforeConflictOfAOnFirst += (nodeForSum.Value as LaboriousDetail).OnFirst.Time.A;
                        }

                    sumBeforeConflictOfAOnFirst += experimentInfo.J12.Sum(detail => detail.OnFirst.Time.A) +
                                                   experimentInfo.J1.Sum(detail => detail.Time.A);

                    var sumOfBOnSecond = sumBeforeConflictOfBOnSecond;
                    var sumOfAOnFirst = sumBeforeConflictOfAOnFirst;

                    var isResolved = true;

                    foreach (var detailInBox in xBox)
                    {
                        sumOfBOnSecond += detailInBox.OnSecond.Time.B;
                        if (sumOfBOnSecond <= sumOfAOnFirst)
                        {
                            sumOfAOnFirst += detailInBox.OnFirst.Time.A;
                            continue;
                        }

                        isResolved = false;
                        break;
                    }

                    if (isResolved)
                    {
                        var insertedNode = node;
                        foreach (var detailInBox in xBox)
                            insertedNode = experimentInfo.J21Chain.AddBefore(node, detailInBox);

                        experimentInfo.J21Chain.Remove(node);
                        node = insertedNode;
                        continue;
                    }

                    var y1Box = conflict.DetailsDictionary.Values
                        .Where(detail => detail.OnSecond.Time.A - detail.OnFirst.Time.B >= 0)
                        .OrderBy(detail => detail.OnFirst.Time.B)
                        .Reverse()
                        .ToList();

                    var y2Box = conflict.DetailsDictionary.Values
                        .Except(y1Box)
                        .OrderBy(detail => detail.OnSecond.Time.A)
                        .ToList();

                    var yBox = y2Box.Concat(y1Box).ToList();

                    double aOfDetailAfterConflict;

                    if (node.Next == null)
                    {
                        aOfDetailAfterConflict = experimentInfo.J2?.Sum(detail => detail.Time.A) ??
                                                 0.0 + experimentInfo.J12?.Sum(detail => detail.OnSecond.Time.A) ?? 0.0;
                        if (aOfDetailAfterConflict == 0.0) break;
                    }
                    else
                    {
                        aOfDetailAfterConflict = node.Next.Value.Type == ChainType.Detail
                            ? (node.Next.Value as LaboriousDetail).OnSecond.Time.A
                            : (node.Next.Value as Conflict).DetailsDictionary.Values.Min(detail =>
                                detail.OnSecond.Time.A);
                    }

                    var sumOfAOnSecond = aOfDetailAfterConflict;
                    var sumOfBOnFirst = 0.0;
                    isResolved = true;
                    foreach (var detailInBox in yBox.AsEnumerable().Reverse())
                    {
                        sumOfBOnFirst += detailInBox.OnFirst.Time.B;
                        if (sumOfAOnSecond >= sumOfBOnFirst)
                        {
                            sumOfAOnSecond += detailInBox.OnSecond.Time.A;
                            continue;
                        }

                        isResolved = false;
                        break;
                    }

                    if (isResolved)
                    {
                        var insertedNode = node;
                        foreach (var detailInBox in yBox)
                            insertedNode = experimentInfo.J21Chain.AddBefore(node, detailInBox);

                        experimentInfo.J21Chain.Remove(node);
                        node = insertedNode;
                    }
                }

            return experimentInfo.IsOptimized;
        }

        private static Chain GetChainFromBox(
            IReadOnlyCollection<LaboriousDetail> sortedBox,
            Func<LaboriousDetail, LaboriousDetail, bool> conflictPredicate)
        {
            if (sortedBox.Count == 1) return new Chain(sortedBox);

            var chain = new Chain();

            foreach (var detailFromSortedBox in sortedBox)
                //chain have elements
                if (chain.Any())
                {
                    //last element from chain
                    var lastElement = chain.Last;
                    if (lastElement.Value.Type == ChainType.Conflict)
                    {
                        var conflict = lastElement.Value as Conflict;
                        var isConflict = conflict.DetailsDictionary.Values.Any(detail =>
                            conflictPredicate.Invoke(detail, detailFromSortedBox));

                        if (isConflict)
                            conflict.DetailsDictionary.Add(detailFromSortedBox.Number, detailFromSortedBox);
                        else
                            chain.AddLast(detailFromSortedBox);
                    }
                    else
                    {
                        var lastDetail = lastElement.Value as LaboriousDetail;
                        if (conflictPredicate.Invoke(lastDetail, detailFromSortedBox))
                        {
                            //it's conflict
                            var conflict = new Conflict();
                            conflict.DetailsDictionary.Add(lastDetail.Number, lastDetail);
                            conflict.DetailsDictionary.Add(detailFromSortedBox.Number, detailFromSortedBox);

                            chain.RemoveLast();
                            chain.AddLast(conflict);
                        }
                        else
                        {
                            chain.AddLast(detailFromSortedBox);
                        }
                    }
                }
                //chain doesn't have elements
                else
                {
                    chain.AddLast(detailFromSortedBox);
                }

            return chain;
        }

        private static Chain TryToOptimizeJ12(ExperimentInfo experimentInfo)
        {
            var boxes = SplitToBoxes(
                experimentInfo.J12,
                detail => detail.OnFirst.Time.B <= detail.OnSecond.Time.A,
                detail => detail.OnSecond.Time.B <= detail.OnFirst.Time.A);

            var sortedFirstBox = boxes.Item1
                .OrderBy(detail => detail.OnFirst.Time.A)
                .ToList();

            var sortedSecondBox = boxes.Item2
                .OrderBy(detail => detail.OnSecond.Time.A)
                .Reverse()
                .ToList();

            var firstChain = GetChainFromBox(
                sortedFirstBox,
                (previousDetail, currentDetail) => previousDetail.OnFirst.Time.B > currentDetail.OnFirst.Time.A);

            var secondChain = GetChainFromBox(
                sortedSecondBox,
                (previousDetail, currentDetail) => previousDetail.OnSecond.Time.A < currentDetail.OnSecond.Time.B);

            if (!boxes.Item3.Any()) return new Chain(firstChain.Concat(secondChain));

            if (boxes.Item3.Count() == 1) return new Chain(firstChain.Append(boxes.Item3.First()).Concat(secondChain));

            //find conflict borders
            var minAOnFirst = boxes.Item3.Min(detail => detail.OnFirst.Time.A);
            var minAOnSecond = boxes.Item3.Min(detail => detail.OnSecond.Time.A);

            var resultChain = new Chain();
            var jConflict = new Conflict();

            foreach (var chainElement in firstChain)
                if (chainElement.Type == ChainType.Conflict)
                {
                    var conflict = chainElement as Conflict;
                    if (conflict.DetailsDictionary.Values.Max(detail => detail.OnFirst.Time.B) > minAOnFirst)
                    {
                        //we find a border of conflict
                        var chainNode = firstChain.Find(chainElement);

                        while (chainNode != null)
                        {
                            var nodeValue = chainNode.Value;
                            if (nodeValue.Type == ChainType.Conflict)
                                jConflict.DetailsDictionary.AddRange((nodeValue as Conflict).DetailsDictionary);
                            else
                            {
                                var laboriousDetail = nodeValue as LaboriousDetail;
                                jConflict.DetailsDictionary.Add(laboriousDetail.Number, laboriousDetail);
                            }

                            chainNode = chainNode.Next;
                        }

                        break;
                    }

                    resultChain.AddLast(chainElement);
                }
                else
                {
                    var detail = chainElement as LaboriousDetail;
                    if (detail.OnFirst.Time.B > minAOnFirst)
                    {
                        //we find a border of conflict
                        var chainNode = firstChain.Find(chainElement);

                        while (chainNode != null)
                        {
                            var nodeValue = chainNode.Value;
                            if (nodeValue.Type == ChainType.Conflict)
                                jConflict.DetailsDictionary.AddRange((nodeValue as Conflict).DetailsDictionary);
                            else
                            {
                                var laboriousDetail = nodeValue as LaboriousDetail;
                                jConflict.DetailsDictionary.Add(laboriousDetail.Number, laboriousDetail);
                            }

                            chainNode = chainNode.Next;
                        }

                        break;
                    }

                    resultChain.AddLast(chainElement);
                }

            jConflict.DetailsDictionary.AddRange(boxes.Item3.Select(x =>
                new KeyValuePair<int, LaboriousDetail>(x.Number, x)));

            foreach (var chainElement in secondChain)
                if (chainElement.Type == ChainType.Conflict)
                {
                    var conflict = chainElement as Conflict;
                    if (conflict.DetailsDictionary.Values.Max(detail => detail.OnSecond.Time.B) > minAOnSecond)
                    {
                        jConflict.DetailsDictionary.AddRange(conflict.DetailsDictionary);
                    }
                    else
                    {
                        resultChain.AddLast(jConflict);
                        jConflict = null;
                        var chainNode = secondChain.Find(chainElement);

                        while (chainNode != null)
                        {
                            resultChain.AddLast(chainNode.Value);
                            chainNode = chainNode.Next;
                        }

                        break;
                    }
                }
                else
                {
                    var detail = chainElement as LaboriousDetail;
                    if (detail.OnSecond.Time.B > minAOnSecond)
                    {
                        jConflict.DetailsDictionary.Add(detail.Number, detail);
                    }
                    else
                    {
                        resultChain.AddLast(jConflict);
                        jConflict = null;
                        var chainNode = secondChain.Find(chainElement);

                        while (chainNode != null)
                        {
                            resultChain.AddLast(chainNode.Value);
                            chainNode = chainNode.Next;
                        }

                        break;
                    }
                }

            if (jConflict != null) resultChain.AddLast(jConflict);

            return resultChain;
        }

        private static Tuple<IEnumerable<LaboriousDetail>, IEnumerable<LaboriousDetail>, IEnumerable<LaboriousDetail>>
            SplitToBoxes(
                LaboriousDetailList details,
                Func<LaboriousDetail, bool> firstBoxSelector,
                Func<LaboriousDetail, bool> secondBoxSelector)
        {
            var firstBox = details
                .Where(firstBoxSelector)
                .ToList();

            var secondBox = details
                .Except(firstBox)
                .Where(secondBoxSelector)
                .ToList();

            var asteriskBox = details
                .Except(firstBox)
                .Except(secondBox)
                .ToList();

            return
                new Tuple<IEnumerable<LaboriousDetail>, IEnumerable<LaboriousDetail>, IEnumerable<LaboriousDetail>>(
                    firstBox, secondBox, asteriskBox);
        }

        private static Chain TryToOptimizeJ21(ExperimentInfo experimentInfo)
        {
            var boxes = SplitToBoxes(
                experimentInfo.J21,
                detail => detail.OnSecond.Time.B <= detail.OnFirst.Time.A,
                detail => detail.OnFirst.Time.B <= detail.OnSecond.Time.A);

            var sortedFirstBox = boxes.Item1
                .OrderBy(detail => detail.OnSecond.Time.A)
                .ToList();

            var sortedSecondBox = boxes.Item2
                .OrderBy(detail => detail.OnFirst.Time.A)
                .Reverse()
                .ToList();

            var firstChain = GetChainFromBox(
                sortedFirstBox,
                (previousDetail, currentDetail) => previousDetail.OnSecond.Time.B > currentDetail.OnSecond.Time.A);

            var secondChain = GetChainFromBox(
                sortedSecondBox,
                (previousDetail, currentDetail) => previousDetail.OnFirst.Time.A < currentDetail.OnFirst.Time.B);

            if (!boxes.Item3.Any()) return new Chain(firstChain.Concat(secondChain));

            if (boxes.Item3.Count() == 1) return new Chain(firstChain.Append(boxes.Item3.First()).Concat(secondChain));

            //find conflict borders
            var minAOnFirst = boxes.Item3.Min(detail => detail.OnFirst.Time.A);
            var minAOnSecond = boxes.Item3.Min(detail => detail.OnSecond.Time.A);

            var resultChain = new Chain();
            var jConflict = new Conflict();

            foreach (var chainElement in firstChain)
                if (chainElement.Type == ChainType.Conflict)
                {
                    var conflict = chainElement as Conflict;
                    if (conflict.DetailsDictionary.Values.Max(detail => detail.OnSecond.Time.B) > minAOnSecond)
                    {
                        //we find a border of conflict
                        var chainNode = firstChain.Find(chainElement);

                        while (chainNode != null)
                        {
                            var nodeValue = chainNode.Value;
                            if (nodeValue.Type == ChainType.Conflict)
                                jConflict.DetailsDictionary.AddRange((nodeValue as Conflict).DetailsDictionary);
                            else
                            {
                                var laboriousDetail = nodeValue as LaboriousDetail;
                                jConflict.DetailsDictionary.Add(laboriousDetail.Number, laboriousDetail);
                            }

                            chainNode = chainNode.Next;
                        }

                        break;
                    }

                    resultChain.AddLast(chainElement);
                }
                else
                {
                    var detail = chainElement as LaboriousDetail;
                    if (detail.OnSecond.Time.B > minAOnSecond)
                    {
                        //we find a border of conflict
                        var chainNode = firstChain.Find(chainElement);

                        while (chainNode != null)
                        {
                            jConflict.AddDetail(chainNode.Value);
                            chainNode = chainNode.Next;
                        }

                        break;
                    }

                    resultChain.AddLast(chainElement);
                }

            jConflict.DetailsDictionary.AddRange(boxes.Item3.Select(x =>
                new KeyValuePair<int, LaboriousDetail>(x.Number, x)));

            foreach (var chainElement in secondChain)
                if (chainElement.Type == ChainType.Conflict)
                {
                    var conflict = chainElement as Conflict;
                    if (conflict.DetailsDictionary.Values.Max(detail => detail.OnFirst.Time.B) > minAOnFirst)
                    {
                        jConflict.DetailsDictionary.AddRange(conflict.DetailsDictionary);
                    }
                    else
                    {
                        resultChain.AddLast(jConflict);
                        jConflict = null;
                        var chainNode = secondChain.Find(chainElement);

                        while (chainNode != null)
                        {
                            resultChain.AddLast(chainNode.Value);
                            chainNode = chainNode.Next;
                        }

                        break;
                    }
                }
                else
                {
                    var detail = chainElement as LaboriousDetail;
                    if (detail.OnFirst.Time.B > minAOnFirst)
                    {
                        jConflict.DetailsDictionary.Add(detail.Number, detail);
                    }
                    else
                    {
                        resultChain.AddLast(jConflict);
                        jConflict = null;
                        var chainNode = secondChain.Find(chainElement);

                        while (chainNode != null)
                        {
                            resultChain.AddLast(chainNode.Value);
                            chainNode = chainNode.Next;
                        }

                        break;
                    }
                }

            if (jConflict != null) resultChain.AddLast(jConflict);

            return resultChain;
        }

        private static void CheckFirst(ExperimentInfo experimentInfo)
        {
            if (experimentInfo.J12.Sum(detail => detail.OnFirst.Time.B) <=
                experimentInfo.J21.Sum(detail => detail.OnSecond.Time.A) +
                experimentInfo.J2.Sum(detail => detail.Time.A))
                experimentInfo.J12.IsOptimized = true;
            else
                return;

            if (experimentInfo.J12.Sum(detail => detail.OnSecond.Time.A) >=
                experimentInfo.J1.Sum(detail => detail.Time.B) +
                experimentInfo.J21.Sum(detail => detail.OnFirst.Time.B))
                experimentInfo.J21.IsOptimized = true;
        }

        private static void CheckSecond(ExperimentInfo experimentInfo)
        {
            if (experimentInfo.J21.Sum(detail => detail.OnSecond.Time.B) <=
                experimentInfo.J12.Sum(detail => detail.OnFirst.Time.A) +
                experimentInfo.J1.Sum(detail => detail.Time.A))
                experimentInfo.J21.IsOptimized = true;
            else
                return;

            //todo if J12 already optimized we don't need check it again
            if (experimentInfo.J21.Sum(detail => detail.OnFirst.Time.A) >=
                experimentInfo.J2.Sum(detail => detail.Time.B) +
                experimentInfo.J12.Sum(detail => detail.OnSecond.Time.B))
                experimentInfo.J12.IsOptimized = true;
        }

        #endregion

        #region Online mode

        private static OnlineChain GetOnlineChainOnFirstMachine(ExperimentInfo experimentInfo)
        {
            var onlineChain = new OnlineChain();
            foreach (var chainNode in experimentInfo.J12Chain)
                switch (chainNode.Type)
                {
                    case ChainType.Detail:
                        onlineChain
                            .AddLast((chainNode as LaboriousDetail).OnFirst);
                        break;
                    case ChainType.Conflict:
                        var onlineConflict = new OnlineConflict();
                        onlineConflict.DetailsDictionary.AddRange(
                            (chainNode as Conflict).DetailsDictionary.Values.Select(d =>
                                new KeyValuePair<int, Detail>(d.OnFirst.Number, d.OnFirst)));

                        onlineChain.AddLast(onlineConflict);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            foreach (var j1 in experimentInfo.J1) onlineChain.AddLast(j1);

            foreach (var chainNode in experimentInfo.J21Chain)
                switch (chainNode.Type)
                {
                    case ChainType.Detail:
                        onlineChain
                            .AddLast((chainNode as LaboriousDetail).OnFirst);
                        break;
                    case ChainType.Conflict:
                        var onlineConflict = new OnlineConflict();
                        onlineConflict.DetailsDictionary.AddRange(
                            (chainNode as Conflict).DetailsDictionary.Values.Select(d =>
                                new KeyValuePair<int, Detail>(d.OnFirst.Number, d.OnFirst)));

                        onlineChain.AddLast(onlineConflict);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            return onlineChain;
        }

        private static OnlineChain GetOnlineChainOnSecondMachine(ExperimentInfo experimentInfo)
        {
            var onlineChain = new OnlineChain();
            foreach (var chainNode in experimentInfo.J21Chain)
                switch (chainNode.Type)
                {
                    case ChainType.Detail:
                        onlineChain
                            .AddLast((chainNode as LaboriousDetail).OnSecond);
                        break;
                    case ChainType.Conflict:
                        var onlineConflict = new OnlineConflict();
                        onlineConflict.DetailsDictionary.AddRange(
                            (chainNode as Conflict).DetailsDictionary.Values.Select(d =>
                                new KeyValuePair<int, Detail>(d.OnSecond.Number, d.OnSecond)));

                        onlineChain.AddLast(onlineConflict);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            foreach (var j2 in experimentInfo.J2) onlineChain.AddLast(j2);

            foreach (var chainNode in experimentInfo.J12Chain)
                switch (chainNode.Type)
                {
                    case ChainType.Detail:
                        onlineChain
                            .AddLast((chainNode as LaboriousDetail).OnSecond);
                        break;
                    case ChainType.Conflict:
                        var onlineConflict = new OnlineConflict();
                        onlineConflict.DetailsDictionary.AddRange(
                            (chainNode as Conflict).DetailsDictionary.Values.Select(d =>
                                new KeyValuePair<int, Detail>(d.OnSecond.Number, d.OnSecond)));

                        onlineChain.AddLast(onlineConflict);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            return onlineChain;
        }

        private static void GeneratePForOnlineChains(ExperimentInfo experimentInfo)
        {
            foreach (var onlineChainNode in experimentInfo.OnlineChainOnFirstMachine)
                switch (onlineChainNode.Type)
                {
                    case OnlineChainType.Detail:
                        (onlineChainNode as Detail).Time.GenerateP();
                        break;
                    case OnlineChainType.Conflict:
                        foreach (var detail in (onlineChainNode as OnlineConflict).DetailsDictionary.Values)
                        {
                            detail.Time.GenerateP();
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            foreach (var onlineChainNode in experimentInfo.OnlineChainOnSecondMachine)
                switch (onlineChainNode.Type)
                {
                    case OnlineChainType.Detail:
                        (onlineChainNode as Detail).Time.GenerateP();
                        break;
                    case OnlineChainType.Conflict:
                        foreach (var detail in (onlineChainNode as OnlineConflict).DetailsDictionary.Values)
                        {
                            detail.Time.GenerateP();
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
        }

        private static void RunOnlineMode(ExperimentInfo experimentInfo)
        {
            GeneratePForOnlineChains(experimentInfo);

            DoRunInOnlineMode(experimentInfo);
        }

        private static void ProcessDetailOnMachine(
            ref IOnlineChainNode currentDetail,
            ICollection<int> processedDetailNumbersOnCurrentMachine,
            ICollection<int> processedDetailNumbersOnAnotherMachine,
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
                (OnlineConflict conflict, ref LinkedListNode<IOnlineChainNode> node, bool isFirst) =>
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    ResolveConflictOnMachine(conflict, ref node, ref anotherMachine, isFirst, chainOnCurrentMachine,
                        chainOnAnotherMachine, machinesStart, result);

                    stopwatch.Stop();

                    result.OnlineExecutionTime =
                        result.OnlineExecutionTime.Add(TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds));
                });

            nodeOnAnotherMachine = anotherMachine;

            if (currentDetail is OnlineConflict) currentDetail = nodeOnCurrentMachine.Value;

            if (nodeOnCurrentMachine.Value is Downtime) currentDetail = null;

            nodeOnCurrentMachine = nodeOnCurrentMachine.Next;
            hasDetailOnCurrentMachine = nodeOnCurrentMachine != null;
        }

        private static void DoRunInOnlineMode(ExperimentInfo experimentInfo)
        {
            var processedDetailNumbersOnFirst = new List<int>();
            processedDetailNumbersOnFirst.AddRange(experimentInfo.J21.Select(d => d.Number));
            processedDetailNumbersOnFirst.AddRange(experimentInfo.J2.Select(d => d.Number));

            var processedDetailNumbersOnSecond = new List<int>();
            processedDetailNumbersOnSecond.AddRange(experimentInfo.J12.Select(d => d.Number));
            processedDetailNumbersOnSecond.AddRange(experimentInfo.J1.Select(d => d.Number));

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
                {
                    if (!(node.Value is Detail))
                        throw new InvalidOperationException(
                            "There can be no conflicts and downtimes when one of machine finished work");

                    time1 += ((Detail) node.Value).Time.P;
                }
            else
                // details only on second machine
                for (var node = nodeOnSecondMachine; node != null; node = node.Next)
                {
                    if (!(node.Value is Detail))
                        throw new InvalidOperationException(
                            "There can be no conflicts and downtimes when one of machine finished work");

                    time2 += ((Detail) node.Value).Time.P;
                }

            timeFromMachinesStart = Math.Max(time1, time2);

            var cMax = timeFromMachinesStart;
            var cOpt = CalculateCOpt(time1, time2, experimentInfo, cMax);

            if (Math.Abs(cOpt - cMax) < 0.0001) experimentInfo.Result.IsStop3OnOnline = true;

            experimentInfo.Result.DeltaCmax = (float) ((cMax - cOpt) / cOpt * 100);
        }

        private static double CalculateCOpt(
            double time1,
            double time2,
            ExperimentInfo experimentInfo,
            double cMax)
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
                        .OrderBy(detail => detail.Time.P);

                    var x2 = j12OnFirstMachine
                        .Except(x1)
                        .OrderByDescending(detail => j12OnSecondMachine.First(d => d.Number == detail.Number).Time.P);

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
                        .OrderBy(detail => j21OnSecondMachine.First(d => d.Number == detail.Number).Time.P);

                    var x2 = j21OnFirstMachine
                        .Except(x1)
                        .OrderByDescending(detail => detail.Time.P);

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
            ICollection<int> processedDetailNumbersOnAnotherMachine,
            ref double time,
            bool isFirstDetail,
            ConflictResolverDelegate conflictResolver)
        {
            var currentDetail = node.Value;

            if (currentDetail.Type == OnlineChainType.Conflict)
            {
                var conflict = currentDetail as OnlineConflict;
                conflictResolver(conflict, ref node, isFirstDetail);

                if (node.Value.Type != OnlineChainType.Detail)
                    throw new InvalidOperationException("Conflict resolver don't change current node");

                currentDetail = node.Value;
            }

            var detail = currentDetail as Detail;
            if (detail == null)
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
            if (!(chainOnAnotherMachine
                    .First(i => i.Type == OnlineChainType.Conflict &&
                                (i as OnlineConflict).DetailsDictionary.Keys
                                .SequenceEqual(conflict.DetailsDictionary.Keys)) is OnlineConflict
                conflictOnAnotherMachine))
                throw new InvalidOperationException("Not found conflict on another machine");

            var conflictNodeOnAnotherMachine = chainOnAnotherMachine.Find(conflictOnAnotherMachine);
            var nodeOnCurrentMachineToRemove = nodeOnCurrentMachine;

            if (!isFirstDetail)
            {
                var sumOfPOnCurrent = chainOnCurrentMachine
                    .TakeWhile(i => !Equals(i, nodeOnCurrentMachineToRemove.Value))
                    .Sum(i =>
                    {
                        switch (i.Type)
                        {
                            case OnlineChainType.Detail:
                                return (i as Detail).Time.P;
                            case OnlineChainType.Downtime:
                                return (i as Downtime).Time;
                            default:
                                throw new InvalidOperationException();
                        }
                    });

                var sumOfBInConflictOnCurrent = conflict.DetailsDictionary.Values.Sum(d => d.Time.B);

                var localNodeOnAnotherMachine = nodeOnAnotherMachine;

                var sumOfPOnAnother = chainOnAnotherMachine
                    .TakeWhile(i => !Equals(i, localNodeOnAnotherMachine.Value))
                    .Sum(i =>
                    {
                        if (i.Type == OnlineChainType.Detail) return (i as Detail).Time.P;

                        if (i.Type == OnlineChainType.Downtime) return (i as Downtime).Time;

                        throw new InvalidOperationException();
                    });

                var l = timeFromMachinesStart - sumOfPOnAnother;
                var sumOnAnother = l <= (nodeOnAnotherMachine.Value as Detail).Time.A
                    ? (nodeOnAnotherMachine.Value as Detail).Time.A
                    : l;

                var sumOfAOnAnother = chainOnAnotherMachine
                    .SkipWhile(i => !Equals(i, localNodeOnAnotherMachine.Value))
                    .Skip(1)
                    .TakeWhile(i => !Equals(i, conflictOnAnotherMachine))
                    .Sum(i =>
                    {
                        if (i.Type == OnlineChainType.Detail) return (i as Detail).Time.A;

                        if (i.Type == OnlineChainType.Conflict)
                            return (i as OnlineConflict).DetailsDictionary.Values.Sum(d => d.Time.A);

                        throw new InvalidOperationException();
                    });

                var sumOnMCurrent = sumOfPOnCurrent + sumOfBInConflictOnCurrent;
                var sumOnMAnother = sumOfPOnAnother + sumOnAnother + sumOfAOnAnother;

                // Check 1
                if (sumOnMCurrent <= sumOnMAnother)
                {
                    nodeOnCurrentMachine = nodeOnCurrentMachineToRemove.Previous;

                    foreach (var conflictDetail in conflict.DetailsDictionary.Values)
                        chainOnCurrentMachine.AddBefore(nodeOnCurrentMachineToRemove, conflictDetail);

                    chainOnCurrentMachine.Remove(nodeOnCurrentMachineToRemove);

                    foreach (var conflictDetail in conflictOnAnotherMachine.DetailsDictionary.Values)
                        chainOnAnotherMachine.AddBefore(conflictNodeOnAnotherMachine, conflictDetail);

                    chainOnAnotherMachine.Remove(conflictNodeOnAnotherMachine);

                    nodeOnCurrentMachine = nodeOnCurrentMachine.Next;

                    result.OnlineResolvedConflictAmount += 1;

                    return;
                }

                // Check 2
                var x1 = conflict.DetailsDictionary.Values
                    .Where(d => conflictOnAnotherMachine.DetailsDictionary[d.Number].Time.A - d.Time.B >=
                                0)
                    .OrderBy(d => d.Time.B);

                var x2 = conflict.DetailsDictionary.Values
                    .Except(x1)
                    .OrderByDescending(d => conflictOnAnotherMachine.DetailsDictionary[d.Number].Time.A);

                var conflictSequence = x1.Concat(x2).ToList();

                var firstSum = sumOfPOnCurrent;
                var secondSum = sumOnMAnother;
                var isOptimized = true;

                foreach (var detail in conflictSequence)
                    //todo check all conditions
                    if (firstSum + detail.Time.B <= secondSum)
                    {
                        firstSum += detail.Time.B;
                        secondSum += conflictOnAnotherMachine.DetailsDictionary[detail.Number].Time.A;
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
                            conflictOnAnotherMachine.DetailsDictionary[detail.Number]);
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
            var t1 = conflict.DetailsDictionary.Values
                .Where(d => d.Time.Average <= conflictOnAnotherMachine.DetailsDictionary[d.Number].Time.Average)
                .OrderBy(d => d.Time.Average);
            var t2 = conflict.DetailsDictionary.Values
                .Except(t1)
                .OrderByDescending(d => conflictOnAnotherMachine.DetailsDictionary[d.Number].Time.Average);

            var conflictSequence = t1.Concat(t2);

            var isFirstAdd = true;
            foreach (var detail in conflictSequence)
                if (isFirstAdd)
                {
                    nodeOnCurrentMachine = chainOnCurrentMachine.AddBefore(nodeOnCurrentMachineToRemove, detail);
                    if (nodeOnAnotherMachine.Value.Type == OnlineChainType.Conflict)
                        nodeOnAnotherMachine = chainOnAnotherMachine.AddBefore(conflictNodeOnAnotherMachine,
                            conflictOnAnotherMachine.DetailsDictionary[detail.Number]);
                    else
                        chainOnAnotherMachine.AddBefore(conflictNodeOnAnotherMachine,
                            conflictOnAnotherMachine.DetailsDictionary[detail.Number]);

                    isFirstAdd = false;
                }
                else
                {
                    chainOnCurrentMachine.AddBefore(nodeOnCurrentMachineToRemove, detail);
                    chainOnAnotherMachine.AddBefore(conflictNodeOnAnotherMachine,
                        conflictOnAnotherMachine.DetailsDictionary[detail.Number]);
                }

            chainOnCurrentMachine.Remove(nodeOnCurrentMachineToRemove);
            chainOnAnotherMachine.Remove(conflictNodeOnAnotherMachine);

            result.OnlineUnResolvedConflictAmount += 1;
            result.IsResolvedOnCheck3InOnline = true;
        }

        #endregion
    }
}