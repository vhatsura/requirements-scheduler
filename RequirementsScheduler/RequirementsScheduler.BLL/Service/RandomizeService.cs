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
            double value;
            do
            {
                value = Accord.Statistics.Distributions.Univariate.GammaDistribution.Random(1, 1, Random);
            } while (value + min > max);

            return value + min;
        }

        public static double GetRandomDouble(int min, int max, Distribution distribution = Distribution.Uniform) =>
            GetRandomDouble(min, (double) max, distribution);
    }
}