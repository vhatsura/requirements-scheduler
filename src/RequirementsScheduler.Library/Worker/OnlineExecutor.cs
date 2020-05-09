using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RequirementsScheduler.BLL.Model;
using RequirementsScheduler.BLL.Model.Conflicts;

namespace RequirementsScheduler.Library.Worker
{
    public class OnlineExecutor : IOnlineExecutor
    {
        public OnlineExecutionContext Execute(OnlineChain onlineChainOnFirst, OnlineChain onlineChainOnSecond,
            HashSet<int> processedDetailsOnFirst, HashSet<int> processedDetailsOnSecond)
        {
            var context = new OnlineExecutionContext();

            IOnlineChainNode currentDetailOnFirst = null;
            IOnlineChainNode currentDetailOnSecond = null;

            var timeFromMachinesStart = 0.0;
            var time1 = 0.0;
            var time2 = 0.0;

            var isFirstDetail = true;

            var nodeOnFirstMachine = onlineChainOnFirst.First;
            var nodeOnSecondMachine = onlineChainOnSecond.First;

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
                    if (hasDetailOnFirst)
                    {
                        ProcessDetailOnMachine(
                            ref currentDetailOnFirst,
                            processedDetailsOnFirst,
                            processedDetailsOnSecond,
                            ref nodeOnFirstMachine,
                            ref nodeOnSecondMachine,
                            timeFromMachinesStart,
                            onlineChainOnFirst,
                            onlineChainOnSecond,
                            ref time1,
                            out hasDetailOnFirst,
                            isFirstDetail,
                            context);
                    }


                    //if (nodeOnSecondMachine?.List == null)
                    //{
                    //    if (isFirstDetail)
                    //        nodeOnSecondMachine = experimentInfo.OnlineChainOnSecondMachine.First;
                    //}

                    if (hasDetailOnSecond)
                    {
                        ProcessDetailOnMachine(
                            ref currentDetailOnSecond,
                            processedDetailsOnSecond,
                            processedDetailsOnFirst,
                            ref nodeOnSecondMachine,
                            ref nodeOnFirstMachine,
                            timeFromMachinesStart,
                            onlineChainOnSecond,
                            onlineChainOnFirst,
                            ref time2,
                            out hasDetailOnSecond,
                            isFirstDetail,
                            context);
                    }
                }
                else if (time1 < time2)
                {
                    ProcessDetailOnMachine(
                        ref currentDetailOnFirst,
                        processedDetailsOnFirst,
                        processedDetailsOnSecond,
                        ref nodeOnFirstMachine,
                        ref nodeOnSecondMachine,
                        timeFromMachinesStart,
                        onlineChainOnFirst,
                        onlineChainOnSecond,
                        ref time1,
                        out hasDetailOnFirst,
                        isFirstDetail,
                        context);
                }
                else
                {
                    ProcessDetailOnMachine(
                        ref currentDetailOnSecond,
                        processedDetailsOnSecond,
                        processedDetailsOnFirst,
                        ref nodeOnSecondMachine,
                        ref nodeOnFirstMachine,
                        timeFromMachinesStart,
                        onlineChainOnSecond,
                        onlineChainOnFirst,
                        ref time2,
                        out hasDetailOnSecond,
                        isFirstDetail,
                        context);
                }

                timeFromMachinesStart = Math.Min(time1, time2);

                isFirstDetail = false;
            }

            // details only on first machines
            if (hasDetailOnFirst)
            {
                for (var node = nodeOnFirstMachine; node != null; node = node.Next)
                    if (node.Value is Detail detail)
                    {
                        time1 += detail.Time.P;
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            "There can be no conflicts and downtimes when one of machine finished work");
                    }
            }
            else
            {
                // details only on second machine
                for (var node = nodeOnSecondMachine; node != null; node = node.Next)
                    if (node.Value is Detail detail)
                    {
                        time2 += detail.Time.P;
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            "There can be no conflicts and downtimes when one of machine finished work");
                    }
            }

            timeFromMachinesStart = Math.Max(time1, time2);

            context.TimeFromMachinesStart = timeFromMachinesStart;
            context.Time1 = time1;
            context.Time2 = time2;

            return context;
        }

        private static void ProcessDetailOnMachine(
            OnlineChain chain,
            ref LinkedListNode<IOnlineChainNode> node,
            Func<Detail, double> downtimeCalculationFunc,
            ISet<int> processedDetailNumbersOnAnotherMachine,
            ref double time,
            bool isFirstDetail,
            OnlineExecutionContext context,
            ConflictResolverDelegate conflictResolver)
        {
            var currentDetail = node.Value;

            if (currentDetail.Type == OnlineChainType.Conflict)
            {
                var conflict = currentDetail as OnlineConflict;

                var start = Stopwatch.GetTimestamp();
                conflictResolver(conflict, ref node, context, isFirstDetail);
                var stop = Stopwatch.GetTimestamp();

                context.ExecutionTime = context.ExecutionTime.Add(
                    TimeSpan.FromMilliseconds((stop - start) / (double) Stopwatch.Frequency * 1000));


                if (node.Value.Type != OnlineChainType.Detail)
                {
                    throw new InvalidOperationException("Conflict resolver didn't change current node");
                }

                currentDetail = node.Value;
            }

            if (!(currentDetail is Detail detail))
            {
                throw new InvalidCastException(
                    $"Try cast {currentDetail.GetType().FullName} to {typeof(Detail).FullName}");
            }

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
            OnlineExecutionContext context)
        {
            if (!(chainOnAnotherMachine.First(i =>
                    i.Type == OnlineChainType.Conflict &&
                    (i as OnlineConflict).Details.Keys.SequenceEqual(conflict.Details.Keys)) is OnlineConflict
                conflictOnAnotherMachine))
            {
                throw new InvalidOperationException("Not found conflict on another machine");
            }

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

                    context.ResolvedConflictAmount += 1;

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

                    context.ResolvedConflictAmount += 1;

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
                    context);

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
                context);
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
            OnlineExecutionContext context)
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

            var conflictSequence = t1.Concat(t2).ToList();

            var isFirstAdd = true;
            foreach (var detail in conflictSequence)
                if (isFirstAdd)
                {
                    nodeOnCurrentMachine = chainOnCurrentMachine.AddBefore(nodeOnCurrentMachineToRemove, detail);
                    if (nodeOnAnotherMachine.Value.Type == OnlineChainType.Conflict)
                    {
                        nodeOnAnotherMachine = chainOnAnotherMachine.AddBefore(conflictNodeOnAnotherMachine,
                            conflictOnAnotherMachine.Details[detail.Number]);
                    }
                    else
                    {
                        chainOnAnotherMachine.AddBefore(conflictNodeOnAnotherMachine,
                            conflictOnAnotherMachine.Details[detail.Number]);
                    }

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

            context.UnresolvedConflictAmount += 1;
            context.IsResolvedOnCheck3InOnline = true;
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
            OnlineExecutionContext context)
        {
            if (currentDetail is Detail detail1)
            {
                processedDetailNumbersOnCurrentMachine.Add(detail1.Number);
            }

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
                context,
                (OnlineConflict conflict, ref LinkedListNode<IOnlineChainNode> node, OnlineExecutionContext context,
                    bool isFirst) =>
                {
                    ResolveConflictOnMachine(conflict, ref node, ref anotherMachine, isFirst, chainOnCurrentMachine,
                        chainOnAnotherMachine, machinesStart, context);
                });

            nodeOnAnotherMachine = anotherMachine;

            if (currentDetail is OnlineConflict)
            {
                currentDetail = nodeOnCurrentMachine.Value;
            }

            if (nodeOnCurrentMachine.Value is Downtime)
            {
                currentDetail = null;
            }

            nodeOnCurrentMachine = nodeOnCurrentMachine.Next;
            hasDetailOnCurrentMachine = nodeOnCurrentMachine != null;
        }

        private delegate void ConflictResolverDelegate(OnlineConflict conflict,
            ref LinkedListNode<IOnlineChainNode> node, OnlineExecutionContext context, bool isFirst);
    }
}
