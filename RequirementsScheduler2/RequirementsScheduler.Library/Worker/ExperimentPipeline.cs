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

        private async Task RunTest(Experiment experiment)
        {
            for (var i = 0; i < experiment.TestsAmount; i++)
            {
                var experimentInfo = await GenerateDataForTest(experiment);

                CheckFirst(experimentInfo);

                if (experimentInfo.J12.IsOptimized && experimentInfo.J21.IsOptimized)
                {
                    experimentInfo.Result.Type = ResultType.STOP1_1;
                    experiment.Results.Add(experimentInfo);
                    continue;
                }

                CheckSecond(experimentInfo);

                if (experimentInfo.J12.IsOptimized && experimentInfo.J21.IsOptimized)
                {
                    experimentInfo.Result.Type = ResultType.STOP1_1;
                    experiment.Results.Add(experimentInfo);
                    continue;
                }
            }
        }

        private void CheckFirst(ExperimentInfo experimentInfo)
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

        private void CheckSecond(ExperimentInfo experimentInfo)
        {
            if (experimentInfo.J21.Sum(detail => detail.OnSecond.Time.B) <=
                    experimentInfo.J12.Sum(detail => detail.OnFirst.Time.A) + experimentInfo.J1.Sum(detail => detail.Time.A))
            {
                experimentInfo.J21.IsOptimized = true;
            }
            else return;

            if (experimentInfo.J21.Sum(detail => detail.OnFirst.Time.A) >=
                experimentInfo.J2.Sum(detail => detail.Time.B) + experimentInfo.J12.Sum(detail => detail.OnSecond.Time.B))
            {
                experimentInfo.J12.IsOptimized = true;
            }
        }

        #region Generation

        private static readonly Random Random = new Random();

        #region Triangle Distribution

        //private Tuple<float[], int, int> InitializeArray(int a, int m, int R, int amount)
        //{
        //    var array = new float[amount];
        //    var period = 0;
        //    var aperiodicInterval = 0;
        //    for (var i = 0; i < amount; i++)
        //    {
        //        R = (a * R) % m;
        //        var value = (float)R / m;
        //        for (var j = 0; j < i; j++)
        //            if (Math.Abs(array[j] - value) < 0.00000001)
        //            {
        //                aperiodicInterval = i;
        //                period = i - j;
        //            }
        //        if (aperiodicInterval != 0)
        //            break;
        //        array[i] = value;
        //    }

        //    return new Tuple<float[], int, int>(array, period, aperiodicInterval);
        //}

        //private float[] EvenNumbersLemer(int a, int m, int R, int amount)
        //{
        //    var lemerObject = InitializeArray(a, m, R, amount);
        //    return lemerObject.Item1;
        //}

        //private float[] GetTriangleDistributionValues(int min, int max, int amount)
        //{
        //    //parameters are calculated in practice
        //    var lemerArray = EvenNumbersLemer(17767, 30893, 32145, amount);

        //    var array = new float[amount];

        //    for (var i = 0; i < Math.Floor((double)lemerArray.Length / 2); i++)
        //    {
        //        array[i] = min + (max - min) * Math.Max(lemerArray[i * 2], lemerArray[2 * i + 1]);
        //    }

        //    return array;
        //}

        #endregion

        private static ICollection<double> GetABoundaries(int min, int max, int amount)
        {
            return Enumerable
                .Range(0, amount)
                .Select(i => Random.Next(min, max) + Random.NextDouble())
                .ToList();
        }

        private static ICollection<double> GetBBoundaries(IEnumerable<double> aBoundaries, int minPercentage,
            int maxPercentage)
        {
            return aBoundaries
                .Select(a => Random.Next(minPercentage, maxPercentage) * a / 100 + a)
                .ToList();
        }

        private Task<ExperimentInfo> GenerateDataForTest(Experiment experiment)
        {
            var firstRequirementsAmount = experiment.RequirementsAmount * experiment.N1 / 100;
            var secondRequirementsAmount = experiment.RequirementsAmount * experiment.N2 / 100;
            var firstSecondRequirementsAmount = experiment.RequirementsAmount * experiment.N12 / 100;
            var secondFirstRequirementsAmount = experiment.RequirementsAmount * experiment.N21 / 100;

            var firstABoundaries = GetABoundaries(experiment.MinBoundaryRange, experiment.MaxBoundaryRange,
                firstRequirementsAmount);
            var firstBBoundaries = GetBBoundaries(firstABoundaries, experiment.MinPercentageFromA,
                experiment.MaxPercentageFromA);

            var secondABoundaries = GetABoundaries(experiment.MinBoundaryRange, experiment.MaxBoundaryRange,
                secondRequirementsAmount);
            var secondBBoundaries = GetBBoundaries(secondABoundaries, experiment.MinPercentageFromA,
                experiment.MaxPercentageFromA);

            var firstSecondFirstABoundaries = GetABoundaries(experiment.MinBoundaryRange, experiment.MaxBoundaryRange,
                firstSecondRequirementsAmount);
            var firstSecondFirstBBoundaries = GetBBoundaries(firstSecondFirstABoundaries, experiment.MinPercentageFromA,
                experiment.MaxPercentageFromA);
            var firstSecondSecondABoundaries = GetABoundaries(experiment.MinBoundaryRange, experiment.MaxBoundaryRange,
                firstSecondRequirementsAmount);
            var firstSecondSecondBBoundaries = GetBBoundaries(firstSecondSecondABoundaries, experiment.MinPercentageFromA,
                experiment.MaxPercentageFromA);

            var secondFirstFirstABoundaries = GetABoundaries(experiment.MinBoundaryRange, experiment.MaxBoundaryRange,
                secondFirstRequirementsAmount);
            var secondFirstFirstBBoundaries = GetBBoundaries(secondFirstFirstABoundaries, experiment.MinPercentageFromA,
                experiment.MaxPercentageFromA);
            var secondFirstSecondABoundaries = GetABoundaries(experiment.MinBoundaryRange, experiment.MaxBoundaryRange,
                secondFirstRequirementsAmount);
            var secondFirstSecondBBoundaries = GetBBoundaries(secondFirstSecondABoundaries, experiment.MinPercentageFromA,
                experiment.MaxPercentageFromA);

            var experimentInfo = new ExperimentInfo();

            experimentInfo.J1.AddRange(firstABoundaries.Zip(firstBBoundaries, (a, b) => new Detail(a, b)));
            experimentInfo.J2.AddRange(secondABoundaries.Zip(secondBBoundaries, (a, b) => new Detail(a, b)));

            var onFirstDetails = firstSecondFirstABoundaries.Zip(firstSecondFirstBBoundaries,
                (a, b) => new Detail(a, b));
            var onSecondDetails = firstSecondSecondABoundaries.Zip(firstSecondSecondBBoundaries,
                (a, b) => new Detail(a, b));

            experimentInfo.J12.AddRange(onFirstDetails.Zip(onSecondDetails, (onFirst, onSecond) => new LaboriousDetail(onFirst, onSecond)));

            onFirstDetails = secondFirstFirstABoundaries.Zip(secondFirstFirstBBoundaries,
                (a, b) => new Detail(a, b));
            onSecondDetails = secondFirstSecondABoundaries.Zip(secondFirstSecondBBoundaries,
                (a, b) => new Detail(a, b));

            experimentInfo.J21.AddRange(onFirstDetails.Zip(onSecondDetails,
                (onFirst, onSecond) => new LaboriousDetail(onFirst, onSecond)));

            return Task.FromResult(experimentInfo);
        }

        #endregion
    }
}
