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
                TryToOptimizeJ12(experimentInfo);
            }

            if (!experimentInfo.J21.IsOptimized)
            {
                TryToOptimizeJ21(experimentInfo);
            }

            return experimentInfo.IsOptimized;
        }

        private static void TryToOptimizeJ12(ExperimentInfo experimentInfo)
        {
            var boxes = SplitToBoxes(
                            experimentInfo.J12,
                            detail => detail.OnFirst.Time.B <= detail.OnSecond.Time.A,
                            detail => detail.OnSecond.Time.B <= detail.OnFirst.Time.A);

            var sortedFirstBox = boxes.Item1
                .OrderBy(detail => detail.OnFirst.Time.A)
                .ToList();

            var sortedSecondBox = boxes.Item2
                .OrderBy(detail => detail.OnFirst.Time.A)
                .Reverse()
                .ToList();

            var firstChain = new LinkedList<LaboriousDetail>();

            if (sortedFirstBox.Count == 1)
            {
                firstChain.AddFirst(sortedFirstBox.First());
            }
            else
            {
                for (var i = 1; i <= sortedFirstBox.Count - 1; i++)
                {
                    if (i == sortedFirstBox.Count - 1)
                    {
                        //todo if it in conflict with last item in chain, then add it to conflict
                        //todo else add as last item to chain
                        if (false)
                        {
                            //todo add it to last conflict
                        }
                        else
                        {
                            firstChain.AddLast(sortedFirstBox[i]);
                        }
                    }

                    if (sortedFirstBox[i - 1].OnFirst.Time.B < sortedFirstBox[i].OnFirst.Time.A)
                    {
                        if (!firstChain.Any())
                        {
                            firstChain.AddFirst(sortedFirstBox[i - 1]);
                        }
                        else
                        {
                            firstChain.AddLast(sortedFirstBox[i - 1]);
                        }
                    }
                    else
                    {
                        //todo add as conflict to LinkedList (i and i + 1)
                        i++;
                    }
                }
            }

            var secondChain = new LinkedList<LaboriousDetail>();
            if (sortedSecondBox.Count == 1)
            {
                secondChain.AddFirst(sortedSecondBox.First());
            }
            else
            {
                for (var i = 1; i <= sortedSecondBox.Count - 1; i++)
                {
                    if (i == sortedSecondBox.Count - 1)
                    {
                        //todo if it in conflict with last item in chain, then add it to conflict
                        //todo else add as last item to chain
                        if (false)
                        {
                            //todo add it to last conflict
                        }
                        else
                        {
                            secondChain.AddLast(sortedSecondBox[i]);
                        }
                    }

                    if (sortedSecondBox[i - 1].OnFirst.Time.B < sortedSecondBox[i].OnFirst.Time.A)
                    {
                        if (!secondChain.Any())
                        {
                            secondChain.AddFirst(sortedSecondBox[i - 1]);
                        }
                        else
                        {
                            secondChain.AddLast(sortedSecondBox[i - 1]);
                        }
                    }
                    else
                    {
                        //todo add as conflict to LinkedList (i and i + 1)
                        i++;
                    }
                }
            }
            
            IEnumerable<LaboriousDetail> chain;

            if (!boxes.Item3.Any())
            {
                chain = firstChain.Concat(secondChain);
            }
            else if (boxes.Item3.Count() == 1)
            {
                chain = firstChain.Append(boxes.Item3.First());
                chain = chain.Concat(secondChain);
            }
            else
            {
                //todo add all details as conflicts
            }

            //todo if we don't have conflicts, sequence is optimized. Else return chain and continue works with chain

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

        private static void TryToOptimizeJ21(ExperimentInfo experimentInfo)
        {
            var boxes = SplitToBoxes(
                            experimentInfo.J21,
                            detail => detail.OnSecond.Time.B <= detail.OnFirst.Time.A,
                            detail => detail.OnFirst.Time.B <= detail.OnSecond.Time.A);

            var sortedFirstBox = boxes.Item1
                    .OrderBy(detail => detail.OnSecond.Time.A)
                    .ToList();

            var sortedSecondBox = boxes.Item2
                .OrderBy(detail => detail.OnSecond.Time.A)
                .Reverse()
                .ToList();

            var firstChain = new LinkedList<LaboriousDetail>();

            for (var i = 1; i <= sortedFirstBox.Count - 1; i++)
            {
                if (i == sortedFirstBox.Count - 1)
                {
                    //todo if it in conflict with last item in chain, then add it to conflict
                    //todo else add as last item to chain
                    if (false)
                    {
                        //todo add it to last conflict
                    }
                    else
                    {
                        firstChain.AddLast(sortedFirstBox[i]);
                    }
                }

                if (sortedFirstBox[i - 1].OnSecond.Time.B < sortedFirstBox[i].OnSecond.Time.A)
                {
                    if (!firstChain.Any())
                    {
                        firstChain.AddFirst(sortedFirstBox[i - 1]);
                    }
                    else
                    {
                        firstChain.AddLast(sortedFirstBox[i - 1]);
                    }
                }
                else
                {
                    //todo add as conflict to LinkedList (i and i + 1)
                    i++;
                }
            }

            var secondChain = new LinkedList<LaboriousDetail>();
            for (var i = 1; i <= sortedSecondBox.Count - 1; i++)
            {
                if (i == sortedSecondBox.Count - 1)
                {
                    //todo if it in conflict with last item in chain, then add it to conflict
                    //todo else add as last item to chain
                    if (false)
                    {
                        //todo add it to last conflict
                    }
                    else
                    {
                        secondChain.AddLast(sortedSecondBox[i]);
                    }
                }

                if (sortedSecondBox[i - 1].OnSecond.Time.B < sortedSecondBox[i].OnSecond.Time.A)
                {
                    if (!secondChain.Any())
                    {
                        secondChain.AddFirst(sortedSecondBox[i - 1]);
                    }
                    else
                    {
                        secondChain.AddLast(sortedSecondBox[i - 1]);
                    }
                }
                else
                {
                    //todo add as conflict to LinkedList (i and i + 1)
                    i++;
                }
            }

            IEnumerable<LaboriousDetail> chain;

            if (!boxes.Item3.Any())
            {
                chain = firstChain.Concat(secondChain);
            }
            else if (boxes.Item3.Count() == 1)
            {
                chain = firstChain.Append(boxes.Item3.First());
                chain = chain.Concat(secondChain);
            }
            else
            {
                //todo add all details as conflicts
            }
            
            //todo if we don't have conflicts, sequence is optimized. Else return chain and continue works with chain
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
