using System;
using System.Collections.Generic;
using System.Linq;
using RequirementsScheduler.BLL.Model;

namespace RequirementsScheduler.BLL.Service
{
    public class ExperimentGenerator : IExperimentGenerator
    {
        private readonly IRandomizeService _randomizeService;

        public ExperimentGenerator(IRandomizeService randomizeService)
        {
            _randomizeService = randomizeService;
        }
        
        public ExperimentInfo GenerateDataForTest(Experiment experiment, int testNumber)
        {
            var firstRequirementsAmount =
                (int) Math.Round(experiment.RequirementsAmount * experiment.N1 / (double) 100);
            var secondRequirementsAmount =
                (int) Math.Round(experiment.RequirementsAmount * experiment.N2 / (double) 100);
            var firstSecondRequirementsAmount =
                (int) Math.Round(experiment.RequirementsAmount * experiment.N12 / (double) 100);
            var secondFirstRequirementsAmount =
                (int) Math.Round(experiment.RequirementsAmount * experiment.N21 / (double) 100);

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
            var firstSecondSecondBBoundaries = GetBBoundaries(firstSecondSecondABoundaries,
                experiment.MinPercentageFromA,
                experiment.MaxPercentageFromA);

            var secondFirstFirstABoundaries = GetABoundaries(experiment.MinBoundaryRange, experiment.MaxBoundaryRange,
                secondFirstRequirementsAmount);
            var secondFirstFirstBBoundaries = GetBBoundaries(secondFirstFirstABoundaries, experiment.MinPercentageFromA,
                experiment.MaxPercentageFromA);
            var secondFirstSecondABoundaries = GetABoundaries(experiment.MinBoundaryRange, experiment.MaxBoundaryRange,
                secondFirstRequirementsAmount);
            var secondFirstSecondBBoundaries = GetBBoundaries(secondFirstSecondABoundaries,
                experiment.MinPercentageFromA,
                experiment.MaxPercentageFromA);

            var experimentInfo = new ExperimentInfo
            {
                TestNumber = testNumber
            };

            var number = 1;

            experimentInfo.J1.AddRange(firstABoundaries.Zip(firstBBoundaries,
                (a, b) => new Detail(a, b, experiment.PGenerationType, number++)));
            experimentInfo.J2.AddRange(secondABoundaries.Zip(secondBBoundaries,
                (a, b) => new Detail(a, b, experiment.PGenerationType, number++)));

            var onFirstDetails = firstSecondFirstABoundaries.Zip(firstSecondFirstBBoundaries,
                (a, b) => new ProcessingTime(a, b, experiment.PGenerationType));
            var onSecondDetails = firstSecondSecondABoundaries.Zip(firstSecondSecondBBoundaries,
                (a, b) => new ProcessingTime(a, b, experiment.PGenerationType));

            experimentInfo.J12.AddRange(onFirstDetails.Zip(onSecondDetails,
                (onFirst, onSecond) => new LaboriousDetail(onFirst, onSecond, number++)));

            onFirstDetails = secondFirstFirstABoundaries.Zip(secondFirstFirstBBoundaries,
                (a, b) => new ProcessingTime(a, b, experiment.PGenerationType));
            onSecondDetails = secondFirstSecondABoundaries.Zip(secondFirstSecondBBoundaries,
                (a, b) => new ProcessingTime(a, b, experiment.PGenerationType));

            experimentInfo.J21.AddRange(onFirstDetails.Zip(onSecondDetails,
                (onFirst, onSecond) => new LaboriousDetail(onFirst, onSecond, number++)));

            return experimentInfo;
        }

        public void GenerateP(IOnlineChainNode node) => node.GenerateP(_randomizeService);
        
        private ICollection<double> GetABoundaries(int min, int max, int amount)
        {
            return Enumerable
                .Range(0, amount)
                .Select(i => _randomizeService.GetRandomDouble(min, max))
                .ToList();
        }

        private ICollection<double> GetBBoundaries(IEnumerable<double> aBoundaries, int minPercentage,
            int maxPercentage)
        {
            return aBoundaries
                .Select(a => _randomizeService.GetRandomDouble(minPercentage, maxPercentage) * a / 100 + a)
                .ToList();
        }

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
    }
}