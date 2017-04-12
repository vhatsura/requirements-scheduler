using System;

namespace RequirementsScheduler.BLL.Service
{
    public static class RandomizeService
    {
        private static readonly Random Random = new Random();

        public static double GetRandomDouble(double min, double max)
        {
            //todo fix cast to int to more complexely algorithm
            return Random.Next((int)min, (int)max) + Random.NextDouble();
        }

        public static double GetRandomDouble(int min, int max) => GetRandomDouble(min, (double)max);
    }
}
