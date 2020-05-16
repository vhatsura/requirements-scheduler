using System;
using RequirementsScheduler.DAL.Model;

namespace RequirementsScheduler.BLL.Service
{
    public class RandomizeService : IRandomizeService
    {
        private static readonly Random Random = new Random();

        public double GetRandomDouble(double min, double max, Distribution distribution)
        {
            return distribution switch
            {
                Distribution.Uniform => Random.Next((int) min, (int) max) + Random.NextDouble(),
                Distribution.Gamma => GammaDistribution(min, max),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public double GetRandomDouble(int min, int max, Distribution distribution = Distribution.Uniform) =>
            GetRandomDouble(min, (double) max, distribution);

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
                value = funcs[0].Invoke();
                // value = funcs[distribution].Invoke();
            } while (distribution == 0 ? value + min > max : max - value < min);

            return distribution == 0 ? value + min : max - value;
        }
    }
}
