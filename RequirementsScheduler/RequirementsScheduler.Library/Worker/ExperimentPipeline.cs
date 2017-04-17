using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RequirementsScheduler.BLL.Model;
using RequirementsScheduler.BLL.Service;
using RequirementsScheduler.Core.Worker;

namespace RequirementsScheduler.Library.Worker
{
    public sealed class ExperimentPipeline : IExperimentPipeline
    {
        private IExperimentGenerator Generator { get; }
        private IWorkerExperimentService Service { get; }
        private IExperimentTestResultService ResultService { get; }

        public ExperimentPipeline(
            IExperimentGenerator generator,
            IWorkerExperimentService service,
            IExperimentTestResultService resultService)
        {
            Generator = generator ?? throw new ArgumentNullException(nameof(generator));
            Service = service;
            ResultService = resultService;
        }

        public async Task Run(IEnumerable<Experiment> experiments)
        {
            foreach (var experiment in experiments)
            {
                experiment.Status = ExperimentStatus.InProgress;
                Service.StartExperiment(experiment.Id);

                await RunTest(experiment);

                experiment.Status = ExperimentStatus.Completed;
                Service.StopExperiment(experiment.Id);
            }
        }

        private Task RunTest(Experiment experiment)
        {
            for (var i = 0; i < experiment.TestsAmount; i++)
            {
                var experimentInfo = Generator.GenerateDataForTest(experiment);

                experimentInfo.TestNumber = i + 1;

                var offlineResult = CheckOffline(experimentInfo);

                if (experimentInfo.J12Chain == null ||
                    experimentInfo.J12.Any() && !experimentInfo.J12Chain.Any())
                {
                    experimentInfo.J12Chain = new Chain(experimentInfo.J12.Select(d => new LaboriousDetail(d.OnFirst, d.OnSecond, d.Number)));
                }
                if (experimentInfo.J21Chain == null ||
                    experimentInfo.J21.Any() && !experimentInfo.J21Chain.Any())
                {
                    experimentInfo.J21Chain = new Chain(experimentInfo.J21.Select(d => new LaboriousDetail(d.OnFirst, d.OnSecond, d.Number)));
                }

                if (!offlineResult)
                {
                    experimentInfo.OnlineChainOnFirstMachine = GetOnlineChainOnFirstMachine(experimentInfo);
                    experimentInfo.OnlineChainOnSecondMachine = GetOnlineChainOnSecondMachine(experimentInfo);

                    RunOnlineMode(experimentInfo);
                }

                ResultService.SaveExperimentTestResult(experiment.Id, experimentInfo);
            }

            return Task.FromResult(0);
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

            if (CheckStopOneAndThree(experimentInfo))
            {
                experimentInfo.Result.Type = ResultType.STOP1_3;
                return true;
            }

            if (CheckStopOneAndFour(experimentInfo))
            {
                experimentInfo.Result.Type = ResultType.STOP1_4;
                return true;
            }

            return false;
        }

        private static bool CheckStopOneAndOne(ExperimentInfo experimentInfo)
        {
            CheckFirst(experimentInfo);

            if (experimentInfo.IsOptimized)
            {
                return true;
            }

            CheckSecond(experimentInfo);
            return experimentInfo.IsOptimized;
        }

        private static bool CheckStopOneAndTwo(ExperimentInfo experimentInfo)
        {
            if (!experimentInfo.J12.IsOptimized)
            {
                experimentInfo.J12Chain = TryToOptimizeJ12(experimentInfo);
            }

            if (!experimentInfo.J21.IsOptimized)
            {
                experimentInfo.J21Chain = TryToOptimizeJ21(experimentInfo);
            }

            return experimentInfo.IsOptimized;
        }

        private static bool CheckStopOneAndThree(ExperimentInfo experimentInfo)
        {
            if (experimentInfo.J12Chain != null && !experimentInfo.J12Chain.IsOptimized)
            {
                for (var node = experimentInfo.J12Chain.First; node != null; node = node.Next)
                {
                    if (node.Value.Type != ChainType.Conflict) continue;

                    var sumOfBOnFirst = 0.0;
                    var sumOfAOnSecond = 0.0;
                    for (var nodeForSum = experimentInfo.J12Chain.First; nodeForSum != node; nodeForSum = nodeForSum.Next)
                    {
                        if (nodeForSum.Value.Type == ChainType.Conflict)
                        {
                            sumOfBOnFirst += (nodeForSum.Value as Conflict).Details.Sum(detail => detail.OnFirst.Time.B);
                            sumOfAOnSecond += (nodeForSum.Value as Conflict).Details.Sum(detail => detail.OnSecond.Time.A);
                        }
                        else
                        {
                            sumOfBOnFirst += (nodeForSum.Value as LaboriousDetail).OnFirst.Time.B;
                            sumOfAOnSecond += (nodeForSum.Value as LaboriousDetail).OnSecond.Time.A;
                        }
                    }
                    sumOfBOnFirst += (node.Value as Conflict).Details.Sum(detail => detail.OnFirst.Time.B);

                    sumOfAOnSecond += experimentInfo.J21.Sum(detail => detail.OnSecond.Time.A) +
                                         experimentInfo.J2.Sum(detail => detail.Time.A);

                    if (sumOfAOnSecond >= sumOfBOnFirst)
                    {
                        LinkedListNode<IChainNode> insertedNode = node;
                        foreach (var laboriousDetail in (node.Value as Conflict).Details)
                        {
                            insertedNode = experimentInfo.J12Chain.AddBefore(node, laboriousDetail);
                        }
                        experimentInfo.J12Chain.Remove(node);
                        node = insertedNode;
                    }
                }
            }

            if (experimentInfo.J21Chain != null && !experimentInfo.J21Chain.IsOptimized)
            {
                for (var node = experimentInfo.J21Chain.First; node != null; node = node.Next)
                {
                    if (node.Value.Type != ChainType.Conflict) continue;

                    var sumOfBOnSecond = 0.0;
                    var sumOfAOnFirst = 0.0;
                    for (var nodeForSum = experimentInfo.J21Chain.First; nodeForSum != node; nodeForSum = nodeForSum.Next)
                    {
                        if (nodeForSum.Value.Type == ChainType.Conflict)
                        {
                            sumOfBOnSecond += (nodeForSum.Value as Conflict).Details.Sum(detail => detail.OnSecond.Time.B);
                            sumOfAOnFirst += (nodeForSum.Value as Conflict).Details.Sum(detail => detail.OnFirst.Time.A);
                        }
                        else
                        {
                            sumOfBOnSecond += (nodeForSum.Value as LaboriousDetail).OnSecond.Time.B;
                            sumOfAOnFirst += (nodeForSum.Value as LaboriousDetail).OnFirst.Time.A;
                        }
                    }
                    sumOfBOnSecond += (node.Value as Conflict).Details.Sum(detail => detail.OnSecond.Time.B);

                    sumOfAOnFirst += experimentInfo.J12.Sum(detail => detail.OnFirst.Time.A) +
                                         experimentInfo.J1.Sum(detail => detail.Time.A);

                    if (sumOfAOnFirst >= sumOfBOnSecond)
                    {
                        LinkedListNode<IChainNode> insertedNode = node;
                        foreach (var laboriousDetail in (node.Value as Conflict).Details)
                        {
                            insertedNode = experimentInfo.J21Chain.AddBefore(node, laboriousDetail);
                        }
                        experimentInfo.J21Chain.Remove(node);
                        node = insertedNode;
                    }
                }
            }

            return experimentInfo.IsOptimized;
        }

        private static bool CheckStopOneAndFour(ExperimentInfo experimentInfo)
        {
            if (experimentInfo.J12Chain != null && !experimentInfo.J12Chain.IsOptimized)
            {
                for (var node = experimentInfo.J12Chain.Last; node != null; node = node.Previous)
                {
                    if (node.Value.Type != ChainType.Conflict) continue;

                    var conflict = node.Value as Conflict;
                    var x1Box = conflict.Details
                        .Where(detail => detail.OnSecond.Time.A - detail.OnFirst.Time.B >= 0)
                        .OrderBy(detail => detail.OnFirst.Time.B)
                        .ToList();

                    var x2Box = conflict.Details
                        .Except(x1Box)
                        .OrderBy(detail => detail.OnSecond.Time.A)
                        .Reverse()
                        .ToList();

                    var xBox = x1Box.Concat(x2Box).ToList();

                    var sumBeforeConflictOfBOnFirst = 0.0;
                    var sumBeforeConflictOfAOnSecond = 0.0;

                    for (var nodeForSum = experimentInfo.J12Chain.First; nodeForSum != node; nodeForSum = nodeForSum.Next)
                    {
                        if (nodeForSum.Value.Type == ChainType.Conflict)
                        {
                            sumBeforeConflictOfBOnFirst += (nodeForSum.Value as Conflict).Details.Sum(detail => detail.OnFirst.Time.B);
                            sumBeforeConflictOfAOnSecond += (nodeForSum.Value as Conflict).Details.Sum(detail => detail.OnSecond.Time.A);
                        }
                        else
                        {
                            sumBeforeConflictOfBOnFirst += (nodeForSum.Value as LaboriousDetail).OnFirst.Time.B;
                            sumBeforeConflictOfAOnSecond += (nodeForSum.Value as LaboriousDetail).OnSecond.Time.A;
                        }
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
                        {
                            insertedNode = experimentInfo.J12Chain.AddBefore(node, detailInBox);
                        }
                        experimentInfo.J12Chain.Remove(node);
                        node = insertedNode;
                        continue;
                    }

                    var y1Box = conflict.Details
                        .Where(detail => detail.OnFirst.Time.A - detail.OnSecond.Time.B >= 0)
                        .OrderBy(detail => detail.OnSecond.Time.B)
                        .Reverse()
                        .ToList();

                    var y2Box = conflict.Details
                        .Except(y1Box)
                        .OrderBy(detail => detail.OnFirst.Time.A)
                        .ToList();

                    var yBox = y2Box.Concat(y1Box).ToList();

                    var aOfDetailAfterConflict = 0.0;

                    if (node.Next == null)
                    {
                        aOfDetailAfterConflict = experimentInfo.J1?.Sum(detail => detail.Time.A) ??
                                                 0.0 + experimentInfo.J21?.Sum(detail => detail.OnFirst.Time.A) ?? 0.0;
                        if (aOfDetailAfterConflict == 0.0)
                        {
                            break;
                        }
                    }
                    else
                    {
                        aOfDetailAfterConflict = node.Next.Value.Type == ChainType.Detail ?
                            (node.Next.Value as LaboriousDetail).OnFirst.Time.A :
                            (node.Next.Value as Conflict).Details.Min(detail => detail.OnFirst.Time.A);
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
                        {
                            insertedNode = experimentInfo.J12Chain.AddBefore(node, detailInBox);
                        }
                        experimentInfo.J12Chain.Remove(node);
                        node = insertedNode;
                    }
                }
            }

            if (experimentInfo.J21Chain != null && !experimentInfo.J21Chain.IsOptimized)
            {
                for (var node = experimentInfo.J21Chain.Last; node != null; node = node.Previous)
                {
                    if (node.Value.Type != ChainType.Conflict) continue;

                    var conflict = node.Value as Conflict;
                    var x1Box = conflict.Details
                        .Where(detail => detail.OnFirst.Time.A - detail.OnSecond.Time.B >= 0)
                        .OrderBy(detail => detail.OnSecond.Time.B)
                        .ToList();

                    var x2Box = conflict.Details
                        .Except(x1Box)
                        .OrderBy(detail => detail.OnFirst.Time.A)
                        .Reverse()
                        .ToList();

                    var xBox = x1Box.Concat(x2Box).ToList();

                    var sumBeforeConflictOfBOnSecond = 0.0;
                    var sumBeforeConflictOfAOnFirst = 0.0;

                    for (var nodeForSum = experimentInfo.J21Chain.First; nodeForSum != node; nodeForSum = nodeForSum.Next)
                    {
                        if (nodeForSum.Value.Type == ChainType.Conflict)
                        {
                            sumBeforeConflictOfBOnSecond += (nodeForSum.Value as Conflict).Details.Sum(detail => detail.OnSecond.Time.B);
                            sumBeforeConflictOfAOnFirst += (nodeForSum.Value as Conflict).Details.Sum(detail => detail.OnFirst.Time.A);
                        }
                        else
                        {
                            sumBeforeConflictOfBOnSecond += (nodeForSum.Value as LaboriousDetail).OnSecond.Time.B;
                            sumBeforeConflictOfAOnFirst += (nodeForSum.Value as LaboriousDetail).OnFirst.Time.A;
                        }
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
                        {
                            insertedNode = experimentInfo.J21Chain.AddBefore(node, detailInBox);
                        }
                        experimentInfo.J21Chain.Remove(node);
                        node = insertedNode;
                        continue;
                    }

                    var y1Box = conflict.Details
                        .Where(detail => detail.OnSecond.Time.A - detail.OnFirst.Time.B >= 0)
                        .OrderBy(detail => detail.OnFirst.Time.B)
                        .Reverse()
                        .ToList();

                    var y2Box = conflict.Details
                        .Except(y1Box)
                        .OrderBy(detail => detail.OnSecond.Time.A)
                        .ToList();

                    var yBox = y2Box.Concat(y1Box).ToList();

                    var aOfDetailAfterConflict = 0.0;

                    if (node.Next == null)
                    {
                        aOfDetailAfterConflict = experimentInfo.J2?.Sum(detail => detail.Time.A) ??
                                                 0.0 + experimentInfo.J12?.Sum(detail => detail.OnSecond.Time.A) ?? 0.0;
                        if (aOfDetailAfterConflict == 0.0)
                        {
                            break;
                        }
                    }
                    else
                    {
                        aOfDetailAfterConflict = node.Next.Value.Type == ChainType.Detail ?
                            (node.Next.Value as LaboriousDetail).OnSecond.Time.A :
                            (node.Next.Value as Conflict).Details.Min(detail => detail.OnSecond.Time.A);
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
                        {
                            insertedNode = experimentInfo.J21Chain.AddBefore(node, detailInBox);
                        }
                        experimentInfo.J21Chain.Remove(node);
                        node = insertedNode;
                    }
                }
            }

            return experimentInfo.IsOptimized;
        }

        private static Chain GetChainFromBox(
            IReadOnlyCollection<LaboriousDetail> sortedBox,
            Func<LaboriousDetail, LaboriousDetail, bool> conflictPredicate)
        {
            if (sortedBox.Count == 1)
            {
                return new Chain(sortedBox);
            }
            var chain = new Chain();

            foreach (var detailFromSortedBox in sortedBox)
            {
                //chain have elements
                if (chain.Any())
                {
                    //last element from chain
                    var lastElement = chain.Last;
                    if (lastElement.Value.Type == ChainType.Conflict)
                    {
                        var conflict = lastElement.Value as Conflict;
                        var isConflict = conflict.Details.Any(detail => conflictPredicate.Invoke(detail, detailFromSortedBox));

                        if (isConflict)
                        {
                            conflict.Details.Add(detailFromSortedBox);
                        }
                        else
                        {
                            chain.AddLast(detailFromSortedBox);
                        }
                    }
                    else
                    {
                        var lastDetail = lastElement.Value as LaboriousDetail;
                        if (conflictPredicate.Invoke(lastDetail, detailFromSortedBox))
                        {
                            //it's conflict
                            var conflict = new Conflict();
                            conflict.Details.Add(lastDetail);
                            conflict.Details.Add(detailFromSortedBox);

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

            if (!boxes.Item3.Any())
            {
                return new Chain(firstChain.Concat(secondChain));
            }

            if (boxes.Item3.Count() == 1)
            {
                return new Chain(firstChain.Append(boxes.Item3.First()).Concat(secondChain));
            }

            //find conflict borders
            var minAOnFirst = boxes.Item3.Min(detail => detail.OnFirst.Time.A);
            var minAOnSecond = boxes.Item3.Min(detail => detail.OnSecond.Time.A);

            var resultChain = new Chain();
            var jConflict = new Conflict();

            foreach (var chainElement in firstChain)
            {
                if (chainElement.Type == ChainType.Conflict)
                {
                    var conflict = chainElement as Conflict;
                    if (conflict.Details.Max(detail => detail.OnFirst.Time.B) > minAOnFirst)
                    {
                        //we find a border of conflict
                        var chainNode = firstChain.Find(chainElement);

                        while (chainNode != null)
                        {
                            var nodeValue = chainNode.Value;
                            if (nodeValue.Type == ChainType.Conflict)
                            {
                                jConflict.Details.AddRange((nodeValue as Conflict).Details);
                            }
                            else
                            {
                                jConflict.Details.Add(nodeValue as LaboriousDetail);
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
                            {
                                jConflict.Details.AddRange((nodeValue as Conflict).Details);
                            }
                            else
                            {
                                jConflict.Details.Add(nodeValue as LaboriousDetail);
                            }
                            chainNode = chainNode.Next;
                        }
                        break;
                    }
                    resultChain.AddLast(chainElement);
                }
            }

            jConflict.Details.AddRange(boxes.Item3);

            foreach (var chainElement in secondChain)
            {
                if (chainElement.Type == ChainType.Conflict)
                {
                    var conflict = chainElement as Conflict;
                    if (conflict.Details.Max(detail => detail.OnSecond.Time.B) > minAOnSecond)
                    {
                        jConflict.Details.AddRange(conflict.Details);
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
                        jConflict.Details.Add(detail);
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
            }

            if (jConflict != null)
            {
                resultChain.AddLast(jConflict);
            }

            return resultChain;
        }

        private static Tuple<IEnumerable<LaboriousDetail>, IEnumerable<LaboriousDetail>, IEnumerable<LaboriousDetail>> SplitToBoxes(
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

            if (!boxes.Item3.Any())
            {
                return new Chain(firstChain.Concat(secondChain));
            }

            if (boxes.Item3.Count() == 1)
            {
                return new Chain(firstChain.Append(boxes.Item3.First()).Concat(secondChain));
            }

            //find conflict borders
            var minAOnFirst = boxes.Item3.Min(detail => detail.OnFirst.Time.A);
            var minAOnSecond = boxes.Item3.Min(detail => detail.OnSecond.Time.A);

            var resultChain = new Chain();
            var jConflict = new Conflict();

            foreach (var chainElement in firstChain)
            {
                if (chainElement.Type == ChainType.Conflict)
                {
                    var conflict = chainElement as Conflict;
                    if (conflict.Details.Max(detail => detail.OnSecond.Time.B) > minAOnSecond)
                    {
                        //we find a border of conflict
                        var chainNode = firstChain.Find(chainElement);

                        while (chainNode != null)
                        {
                            var nodeValue = chainNode.Value;
                            if (nodeValue.Type == ChainType.Conflict)
                            {
                                jConflict.Details.AddRange((nodeValue as Conflict).Details);
                            }
                            else
                            {
                                jConflict.Details.Add(nodeValue as LaboriousDetail);
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
                            var nodeValue = chainNode.Value;
                            if (nodeValue.Type == ChainType.Conflict)
                            {
                                jConflict.Details.AddRange((nodeValue as Conflict).Details);
                            }
                            else
                            {
                                jConflict.Details.Add(nodeValue as LaboriousDetail);
                            }
                            chainNode = chainNode.Next;
                        }
                        break;
                    }
                    resultChain.AddLast(chainElement);
                }
            }

            jConflict.Details.AddRange(boxes.Item3);

            foreach (var chainElement in secondChain)
            {
                if (chainElement.Type == ChainType.Conflict)
                {
                    var conflict = chainElement as Conflict;
                    if (conflict.Details.Max(detail => detail.OnFirst.Time.B) > minAOnFirst)
                    {
                        jConflict.Details.AddRange(conflict.Details);
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
                        jConflict.Details.Add(detail);
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
            }

            if (jConflict != null)
            {
                resultChain.AddLast(jConflict);
            }

            return resultChain;
        }

        private static void CheckFirst(ExperimentInfo experimentInfo)
        {
            if (experimentInfo.J12.Sum(detail => detail.OnFirst.Time.B) <=
                    experimentInfo.J21.Sum(detail => detail.OnSecond.Time.A) + experimentInfo.J2.Sum(detail => detail.Time.A))
            {
                experimentInfo.J12.IsOptimized = true;
            }
            else
                return;

            if (experimentInfo.J12.Sum(detail => detail.OnSecond.Time.A) >=
                experimentInfo.J1.Sum(detail => detail.Time.B) + experimentInfo.J21.Sum(detail => detail.OnFirst.Time.B))
            {
                experimentInfo.J21.IsOptimized = true;
            }
        }

        private static void CheckSecond(ExperimentInfo experimentInfo)
        {
            if (experimentInfo.J21.Sum(detail => detail.OnSecond.Time.B) <=
                    experimentInfo.J12.Sum(detail => detail.OnFirst.Time.A) + experimentInfo.J1.Sum(detail => detail.Time.A))
            {
                experimentInfo.J21.IsOptimized = true;
            }
            else
                return;

            //todo if J12 already optimized we don't need check it again
            if (experimentInfo.J21.Sum(detail => detail.OnFirst.Time.A) >=
                experimentInfo.J2.Sum(detail => detail.Time.B) + experimentInfo.J12.Sum(detail => detail.OnSecond.Time.B))
            {
                experimentInfo.J12.IsOptimized = true;
            }
        }

        #endregion

        #region Online mode

        private static OnlineChain GetOnlineChainOnFirstMachine(ExperimentInfo experimentInfo)
        {
            var onlineChain = new OnlineChain();
            foreach (var chainNode in experimentInfo.J12Chain)
            {
                switch (chainNode.Type)
                {
                    case ChainType.Detail:
                        onlineChain
                            .AddLast((chainNode as LaboriousDetail).OnFirst);
                        break;
                    case ChainType.Conflict:
                        var onlineConflict = new Conflict();
                        onlineConflict.Details.AddRange((chainNode as Conflict).Details);

                        onlineChain.AddLast(onlineConflict);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            foreach (var j1 in experimentInfo.J1)
            {
                onlineChain.AddLast(j1);
            }

            foreach (var chainNode in experimentInfo.J21Chain)
            {
                switch (chainNode.Type)
                {
                    case ChainType.Detail:
                        onlineChain
                            .AddLast((chainNode as LaboriousDetail).OnFirst);
                        break;
                    case ChainType.Conflict:
                        var onlineConflict = new Conflict();
                        onlineConflict.Details.AddRange((chainNode as Conflict).Details);

                        onlineChain.AddLast(onlineConflict);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return onlineChain;
        }

        private static OnlineChain GetOnlineChainOnSecondMachine(ExperimentInfo experimentInfo)
        {
            var onlineChain = new OnlineChain();
            foreach (var chainNode in experimentInfo.J21Chain)
            {
                switch (chainNode.Type)
                {
                    case ChainType.Detail:
                        onlineChain
                            .AddLast((chainNode as LaboriousDetail).OnSecond);
                        break;
                    case ChainType.Conflict:
                        var onlineConflict = new Conflict();
                        onlineConflict.Details.AddRange((chainNode as Conflict).Details);

                        onlineChain.AddLast(onlineConflict);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            foreach (var j2 in experimentInfo.J2)
            {
                onlineChain.AddLast(j2);
            }

            foreach (var chainNode in experimentInfo.J12Chain)
            {
                switch (chainNode.Type)
                {
                    case ChainType.Detail:
                        onlineChain
                            .AddLast((chainNode as LaboriousDetail).OnSecond);
                        break;
                    case ChainType.Conflict:
                        var onlineConflict = new Conflict();
                        onlineConflict.Details.AddRange((chainNode as Conflict).Details);

                        onlineChain.AddLast(onlineConflict);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return onlineChain;
        }

        private static void GeneratePForOnlineChains(ExperimentInfo experimentInfo)
        {
            foreach (var onlineChainNode in experimentInfo.OnlineChainOnFirstMachine)
            {
                switch (onlineChainNode.Type)
                {
                    case OnlineChainType.Detail:
                        (onlineChainNode as Detail).Time.GenerateP();
                        break;
                    case OnlineChainType.Conflict:
                        (onlineChainNode as Conflict).Details.ForEach(detail => detail.OnFirst.Time.GenerateP());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            foreach (var onlineChainNode in experimentInfo.OnlineChainOnSecondMachine)
            {
                switch (onlineChainNode.Type)
                {
                    case OnlineChainType.Detail:
                        (onlineChainNode as Detail).Time.GenerateP();
                        break;
                    case OnlineChainType.Conflict:
                        (onlineChainNode as Conflict).Details.ForEach(detail => detail.OnSecond.Time.GenerateP());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static void RunOnlineMode(ExperimentInfo experimentInfo)
        {
            GeneratePForOnlineChains(experimentInfo);

            DoRunInOnlineMode(experimentInfo);


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

            bool isFirstDetail = true;

            var nodeOnFirstMachine = experimentInfo.OnlineChainOnFirstMachine.First;

            var nodeOnSecondMachine = experimentInfo.OnlineChainOnSecondMachine.First;

            var hasDetailOnFirst = nodeOnFirstMachine != null;
            var hasDetailOnSecond = nodeOnSecondMachine != null;

            // details are on two machines
            while (hasDetailOnFirst && hasDetailOnSecond)
            {
                var start = timeFromMachinesStart;

                // time1 equal to time2
                if (Math.Abs(time1 - time2) < 0.001)
                {
                    processedDetailNumbersOnFirst.Add((currentDetailOnFirst as Detail).Number);
                    currentDetailOnFirst = nodeOnFirstMachine.Value;

                    var machine = nodeOnSecondMachine;
                    ProcessDetailOnMachine(
                        experimentInfo.OnlineChainOnFirstMachine,
                        nodeOnFirstMachine,
                        detail => experimentInfo.OnlineChainOnSecondMachine
                                           .TakeWhile(d => (d as Detail).Number != detail.Number)
                                           .Sum(d => (d as Detail).Time.P) +
                                       (experimentInfo.OnlineChainOnSecondMachine.First(
                                           d => d is Detail && (d as Detail).Number == detail.Number) as Detail).Time.P - start,
                        processedDetailNumbersOnSecond,
                        ref time1,
                        isFirstDetail,
                        (conflict, node, isFirst) => ResolveConflictOnFirst(conflict, node, machine, isFirst, experimentInfo, timeFromMachinesStart));

                    processedDetailNumbersOnSecond.Add((currentDetailOnSecond as Detail).Number);
                    currentDetailOnSecond = nodeOnSecondMachine.Value;
                    ProcessDetailOnMachine(
                        experimentInfo.OnlineChainOnSecondMachine,
                        nodeOnSecondMachine,
                        detail => experimentInfo.OnlineChainOnFirstMachine
                                            .TakeWhile(d => (d as Detail).Number != detail.Number)
                                            .Sum(d => (d as Detail).Time.P) +
                                        (experimentInfo.OnlineChainOnFirstMachine.First(
                                            d => d is Detail && (d as Detail).Number == detail.Number) as Detail).Time.P - start,
                        processedDetailNumbersOnFirst,
                        ref time2,
                        isFirstDetail,
                        ResolveConflictOnSecond);

                    nodeOnFirstMachine = nodeOnFirstMachine.Next;
                    hasDetailOnFirst = nodeOnFirstMachine != null;

                    nodeOnSecondMachine = nodeOnSecondMachine.Next;
                    hasDetailOnSecond = nodeOnSecondMachine != null;
                }
                else if (time1 < time2)
                {
                    processedDetailNumbersOnFirst.Add((currentDetailOnFirst as Detail).Number);
                    currentDetailOnFirst = nodeOnFirstMachine.Value;

                    var machine = nodeOnSecondMachine;
                    ProcessDetailOnMachine(
                        experimentInfo.OnlineChainOnFirstMachine,
                        nodeOnFirstMachine,
                        detail => experimentInfo.OnlineChainOnSecondMachine
                                      .TakeWhile(d => (d as Detail).Number != detail.Number)
                                      .Sum(d => (d as Detail).Time.P) +
                                  (experimentInfo.OnlineChainOnSecondMachine.First(
                                      d => d is Detail && (d as Detail).Number == detail.Number) as Detail).Time.P - start,
                        processedDetailNumbersOnSecond,
                        ref time1,
                        isFirstDetail,
                        (conflict, node, isFirst) => ResolveConflictOnFirst(conflict, node, machine, isFirst, experimentInfo, timeFromMachinesStart));

                    nodeOnFirstMachine = nodeOnFirstMachine.Next;
                    hasDetailOnFirst = nodeOnFirstMachine != null;
                }
                else
                {
                    processedDetailNumbersOnSecond.Add((currentDetailOnSecond as Detail).Number);
                    currentDetailOnSecond = nodeOnSecondMachine.Value;
                    ProcessDetailOnMachine(
                        experimentInfo.OnlineChainOnSecondMachine,
                        nodeOnSecondMachine,
                        detail => experimentInfo.OnlineChainOnFirstMachine
                                      .TakeWhile(d => (d as Detail).Number != detail.Number)
                                      .Sum(d => (d as Detail).Time.P) +
                                  (experimentInfo.OnlineChainOnFirstMachine.First(
                                      d => d is Detail && (d as Detail).Number == detail.Number) as Detail).Time.P - start,
                        processedDetailNumbersOnFirst,
                        ref time2,
                        isFirstDetail,
                        ResolveConflictOnSecond);

                    nodeOnSecondMachine = nodeOnSecondMachine.Next;
                    hasDetailOnSecond = nodeOnSecondMachine != null;
                }

                timeFromMachinesStart = Math.Min(time1, time2);

                isFirstDetail = false;
            }

            //todo add downtimes to chains

            // details aren't on two machines
            if (!hasDetailOnFirst && !hasDetailOnSecond)
            {
                return;
            }
            // details only on first machines
            if (hasDetailOnFirst)
            {
                return;
            }
            // details only on second machine
            return;

        }

        private static void ProcessDetailOnMachine(
            OnlineChain chain,
            LinkedListNode<IOnlineChainNode> node,
            Func<Detail, double> downtimeCalculationFunc,
            ICollection<int> processedDetailNumbersOnAnotherMachine,
            ref double time,
            bool isFirstDetail,
            Action<Conflict, LinkedListNode<IOnlineChainNode>, bool> conflictResolver)
        {
            var currentDetail = node.Value;
            if (currentDetail.Type != OnlineChainType.Conflict)
            {
                var detail = currentDetail as Detail;
                if (detail == null)
                    throw new InvalidCastException($"Try cast {currentDetail.GetType().FullName} to {typeof(Detail).FullName}");

                if (processedDetailNumbersOnAnotherMachine.Contains(detail.Number))
                {
                    time += detail.Time.P;
                }
                else
                {
                    var downTime = downtimeCalculationFunc(detail);

                    chain.AddBefore(node, new Downtime(downTime));
                    
                    time += downTime;
                }

            }
            else
            {
                var conflict = currentDetail as Conflict;
                conflictResolver(conflict, node, isFirstDetail);
                
            }
        }

        private static void ResolveConflictOnFirst(
            Conflict conflict,
            LinkedListNode<IOnlineChainNode> nodeOnFirst,
            LinkedListNode<IOnlineChainNode> nodeOnSecond,
            bool isFirstDetail,
            ExperimentInfo experimentInfo,
            double timeFromMachinesStart)
        {
            if (!isFirstDetail)
            {
                var sumOfPOnFirst = experimentInfo.OnlineChainOnFirstMachine
                    .TakeWhile(i => !Equals(i, nodeOnFirst.Value))
                    .Sum(i =>
                    {
                        if (i.Type == OnlineChainType.Detail)
                        {
                            return (i as Detail).Time.P;
                        }
                        if (i.Type == OnlineChainType.Downtime)
                        {
                            return (i as Downtime).Time;
                        }

                        throw new InvalidOperationException();
                    });

                var sumOfBInConflict = conflict.Details.Sum(d => d.OnFirst.Time.B);

                var sumOfPOnSecond = experimentInfo.OnlineChainOnSecondMachine
                    .TakeWhile(i => !Equals(i, nodeOnSecond.Value))
                    .Sum(i =>
                    {
                        if (i.Type == OnlineChainType.Detail)
                        {
                            return (i as Detail).Time.P;
                        }
                        if (i.Type == OnlineChainType.Downtime)
                        {
                            return (i as Downtime).Time;
                        }

                        throw new InvalidOperationException();
                    });

                double sumOnSecond;

                var l = timeFromMachinesStart - sumOfPOnSecond;
                if (l <= (nodeOnSecond.Value as Detail).Time.A)
                {
                    sumOnSecond = sumOfPOnSecond + (nodeOnSecond.Value as Detail).Time.A;
                }
                else
                {
                    sumOnSecond = sumOfPOnSecond + l;
                }

                var conflictOnSecond = experimentInfo.OnlineChainOnSecondMachine
                    .First(i => i.Type == OnlineChainType.Conflict &&
                                (i as Conflict).Details
                                                     .Select(d => d.Number)
                                                     .SequenceEqual(conflict.Details.Select(d => d.Number))) as Conflict;

                var conflictNodeOnSecond = experimentInfo.OnlineChainOnSecondMachine.Find(conflictOnSecond);

                var sumOfAOnSecond = experimentInfo.OnlineChainOnSecondMachine
                    .SkipWhile(i => !Equals(i, nodeOnSecond.Value))
                    .Skip(1)
                    .TakeWhile(i => !Equals(i, conflictOnSecond))
                    .Sum(i =>
                    {
                        if (i.Type == OnlineChainType.Detail)
                        {
                            return (i as Detail).Time.A;
                        }
                        if (i.Type == OnlineChainType.Conflict)
                        {
                            return (i as Conflict).Details.Sum(d => d.OnSecond.Time.A);
                        }

                        throw new InvalidOperationException();
                    });

                var sumOnMFirst = sumOfPOnFirst + sumOfBInConflict;
                var sumOnMSecond = sumOfPOnSecond + sumOnSecond + sumOfAOnSecond;

                // Check 1
                if (sumOnMFirst <= sumOnMSecond)
                {
                    foreach (var conflictDetail in conflict.Details)
                    {
                        experimentInfo.OnlineChainOnFirstMachine.AddBefore(nodeOnFirst, conflictDetail.OnFirst);
                    }
                    experimentInfo.OnlineChainOnFirstMachine.Remove(nodeOnFirst);

                    foreach (var conflictDetail in conflictOnSecond.Details)
                    {
                        experimentInfo.OnlineChainOnSecondMachine.AddBefore(conflictNodeOnSecond, conflictDetail.OnSecond);
                    }
                    experimentInfo.OnlineChainOnSecondMachine.Remove(conflictNodeOnSecond);

                    return;
                }

                // Check 2
                var x1 = conflict.Details
                    .Where(d => d.OnSecond.Time.A - d.OnFirst.Time.B >= 0)
                    .OrderBy(d => d.OnFirst.Time.B);

                var x2 = conflict.Details
                    .Except(x1)
                    .OrderByDescending(d => d.OnSecond.Time.A);

                var conflictSequence = x1.Concat(x2);

                var firstSum = sumOfPOnFirst;
                var secondSum = sumOnMSecond;
                var isOptimized = true;

                foreach (var laboriousDetail in conflictSequence)
                {
                    //todo check all conditions
                    if (firstSum + laboriousDetail.OnFirst.Time.B <= secondSum)
                    {
                        firstSum += laboriousDetail.OnFirst.Time.B;
                        secondSum += laboriousDetail.OnSecond.Time.A;
                    }
                    else
                    {
                        isOptimized = false;
                        break;
                    }
                }

                if (isOptimized)
                {
                    foreach (var laboriousDetail in conflictSequence)
                    {
                        experimentInfo.OnlineChainOnFirstMachine.AddBefore(nodeOnFirst, laboriousDetail.OnFirst);
                        experimentInfo.OnlineChainOnSecondMachine.AddBefore(conflictNodeOnSecond, laboriousDetail.OnSecond);
                    }

                    experimentInfo.OnlineChainOnFirstMachine.Remove(nodeOnFirst);
                    experimentInfo.OnlineChainOnSecondMachine.Remove(conflictNodeOnSecond);
                    return;
                }
                // Check 3
            }
            else
            {
                // Check 3
                // todo resolve conflict using Jonson algorithm (3rd check)
            }
        }

        private static void ResolveConflictOnSecond(
            Conflict conflict,
            LinkedListNode<IOnlineChainNode> node,
            bool isFirstDetail)
        {
            if (isFirstDetail)
            {
                // todo resolve conflict using Jonson algorithm (3rd check)
            }
            else
            {

            }
        }

        #endregion
    }
}

