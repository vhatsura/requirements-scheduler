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
                await GenerateDataForTest(experiment);
            }
        }

        private static readonly Random Random = new Random();

        #region Triangle Distribution

        private Tuple<float[], int, int> InitializeArray(int a, int m, int R, int amount)
        {
            var array = new float[amount];
            var period = 0;
            var aperiodicInterval = 0;
            for (var i = 0; i < amount; i++)
            {
                R = (a * R) % m;
                var value = (float)R / m;
                for (var j = 0; j < i; j++)
                    if (Math.Abs(array[j] - value) < 0.00000001)
                    {
                        aperiodicInterval = i;
                        period = i - j;
                    }
                if (aperiodicInterval != 0)
                    break;
                array[i] = value;
            }

            return new Tuple<float[], int, int>(array, period, aperiodicInterval);
        }

        private float[] EvenNumbersLemer(int a, int m, int R, int amount)
        {
            var lemerObject = InitializeArray(a, m, R, amount);
            return lemerObject.Item1;
        }

        private float[] GetTriangleDistributionValues(int min, int max, int amount)
        {
            //parameters are calculated in practice
            var lemerArray = EvenNumbersLemer(17767, 30893, 32145, amount);

            var array = new float[amount];

            for (var i = 0; i < Math.Floor((double)lemerArray.Length / 2); i++)
            {
                array[i] = min + (max - min) * Math.Max(lemerArray[i * 2], lemerArray[2 * i + 1]);
            }

            return array;
        }

        #endregion

        private static IEnumerable<double> GetABoundaries(int min, int max, int amount)
        {
            return Enumerable
                .Range(0, amount)
                .Select(i => Random.Next(min, max) + Random.NextDouble())
                .ToList();
        }

        private Task GenerateDataForTest(Experiment experiment)
        {
            var firstRequirementsAmount = experiment.RequirementsAmount * experiment.N1 / 100;
            var secondRequirementsAmount = experiment.RequirementsAmount * experiment.N2 / 100;
            var firstSecondRequirementsAmount = experiment.RequirementsAmount * experiment.N12 / 100;
            var secondFirstRequirementsAmount = experiment.RequirementsAmount * experiment.N21 / 100;

            var firstABoundaries = GetABoundaries(experiment.MinBoundaryRange, experiment.MaxBoundaryRange, firstRequirementsAmount);
            var secondABoundaries = GetABoundaries(experiment.MinBoundaryRange, experiment.MaxBoundaryRange, secondRequirementsAmount);


            return Task.FromResult(0);
        }
    }
}
