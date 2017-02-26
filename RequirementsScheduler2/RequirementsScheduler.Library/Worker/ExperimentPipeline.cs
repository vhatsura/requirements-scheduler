using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RequirementsScheduler.DAL.Repository;
using RequirementsScheduler.Core.Model;

namespace RequirementsScheduler.Core.Worker
{
    public sealed class ExperimentPipeline
    {
        private readonly IRepository<Experiment> Repository = new ExperimentsRepository();

        private IExperimentGenerator Generator { get; }

        public ExperimentPipeline(IExperimentGenerator generator)
        {
            Generator = generator;
        }

        public async Task Run(IEnumerable<Experiment> experiments)
        {
            foreach (var experiment in experiments)
            {
                experiment.Status = ExperimentStatus.InProgress;
                Repository.Update(experiment);

                await RunTest(experiment);

                experiment.Status = ExperimentStatus.Completed;
                Repository.Update(experiment);
            }
        }

        private Task RunTest(Experiment experiment)
        {
            for (var i = 0; i < experiment.TestsAmount; i++)
            {
                var experimentInfo = Generator.GenerateDataForTest(experiment);

                if (CheckStopOneAndOne(experimentInfo))
                {
                    experimentInfo.Result.Type = ResultType.STOP1_1;
                    experiment.Results.Add(experimentInfo);
                    continue;
                }

                if (CheckStopOneAndTwo(experimentInfo))
                {
                    experimentInfo.Result.Type = ResultType.STOP1_2;
                    experiment.Results.Add(experimentInfo);
                    continue;
                }

                if (CheckStopOneAndThree(experimentInfo))
                {
                    experimentInfo.Result.Type = ResultType.STOP1_3;
                    experiment.Results.Add(experimentInfo);
                    continue;
                }

                if (CheckStopOneAndFour(experimentInfo))    
                {
                    experimentInfo.Result.Type = ResultType.STOP1_4;
                    experiment.Results.Add(experimentInfo);
                    continue;
                }

            }

            return Task.FromResult(0);
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
                    if(node.Value.Type != ChainType.Conflict) continue;

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
                        if (experimentInfo.J1.Any())
                        {
                            aOfDetailAfterConflict = experimentInfo.J1.First().Time.A;
                        }
                        else
                        {
                            break;
                            //throw new NotSupportedException();
                        }
                    }
                    else
                    {
                        aOfDetailAfterConflict = node.Next.Value.Type == ChainType.Detail ? 
                            (node.Next.Value as LaboriousDetail).OnFirst.Time.A : 
                            (node.Next.Value as Conflict).Details.First().OnFirst.Time.A;
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
                        if (experimentInfo.J2.Any())
                        {
                            aOfDetailAfterConflict = experimentInfo.J2.First().Time.A;
                        }
                        else
                        {
                            break;
                            //throw new NotSupportedException();
                        }
                    }
                    else
                    {
                        aOfDetailAfterConflict = node.Next.Value.Type == ChainType.Detail ?
                            (node.Next.Value as LaboriousDetail).OnSecond.Time.A :
                            (node.Next.Value as Conflict).Details.First().OnSecond.Time.A;
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

            return new Tuple<IEnumerable<LaboriousDetail>, IEnumerable<LaboriousDetail>, IEnumerable<LaboriousDetail>>(
                firstBox,secondBox, asteriskBox);
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

            return resultChain;
        }

        private static void CheckFirst(ExperimentInfo experimentInfo)
        {
            if (experimentInfo.J12.Sum(detail => detail.OnFirst.Time.B) <=
                    experimentInfo.J21.Sum(detail => detail.OnSecond.Time.A) + experimentInfo.J2.Sum(detail => detail.Time.A))
            {
                experimentInfo.J12.IsOptimized = true;
            }
            else return;

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
            else return;
            
            //todo if J12 already optimized we don't need check it again
            if (experimentInfo.J21.Sum(detail => detail.OnFirst.Time.A) >=
                experimentInfo.J2.Sum(detail => detail.Time.B) + experimentInfo.J12.Sum(detail => detail.OnSecond.Time.B))
            {
                experimentInfo.J12.IsOptimized = true;
            }
        }
    }
}
