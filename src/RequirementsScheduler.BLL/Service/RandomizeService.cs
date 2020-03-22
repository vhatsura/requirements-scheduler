using System;
using RequirementsScheduler.DAL.Model;

namespace RequirementsScheduler.BLL.Service
{
    public static class RandomizeService
    {
        private static readonly Random Random = new Random();

        public static double GetRandomDouble(double min, double max, Distribution distribution)
        {
            return distribution switch
            {
                Distribution.Uniform => Random.Next((int) min, (int) max) + Random.NextDouble(),
                Distribution.Gamma => GammaDistribution(min, max),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static double GammaDistribution(double min, double max)
        {
            var funcs = new Func<double>[2]
            {
                () => Accord.Statistics.Distributions.Univariate.GammaDistribution.Random(0.5, 1, Random),
                () => Accord.Statistics.Distributions.Univariate.GammaDistribution.Random(7.5, 3, Random)
            };

            var distribution = Random.Next(0, 2);

            double value;
            do
            {
                value = funcs[distribution].Invoke();
            } while (value + min > max);

            return value + min;
        }

        public static double GetRandomDouble(int min, int max, Distribution distribution = Distribution.Uniform) =>
            GetRandomDouble(min, (double) max, distribution);
    }
}