using System;
using System.Collections.Generic;
using RequirementsScheduler.Core.Model;
using RequirementsScheduler.Core.Worker;

namespace RequirementsScheduler.Host.Console
{
    public class Program
    {
        private static bool ConvertValue<T>(string value, out T result)
        {
            try
            {
                result = (T) Convert.ChangeType(value, typeof(T));
                return true;
            }
            catch (Exception)
            {
                result = default(T);
                return false;
            }
        }

        private static void ReadFromConsole<T>(Action<T> setter, string message)
        {
            System.Console.Write(message);
            var line = System.Console.ReadLine();
            T value;

            if (!ConvertValue(line, out value))
            {
                System.Console.Write("Error input");
                return;
            }

            setter.Invoke(value);
        }

        public static Experiment ReadExperimentFromConsole()
        {
            var experiment = new Experiment();

            ReadFromConsole<int>(result => experiment.TestsAmount = result, "Enter amount of tests: ");
            ReadFromConsole<int>(result => experiment.RequirementsAmount = result, "Enter amount of requirements: ");

            ReadFromConsole<int>(result => experiment.MinBoundaryRange = result, "Enter min boundary range: ");
            ReadFromConsole<int>(result => experiment.MaxBoundaryRange = result, "Enter max boundary range: ");

            ReadFromConsole<int>(result => experiment.MinPercentageFromA = result, "Enter min percentage from A boundary: ");
            ReadFromConsole<int>(result => experiment.MaxPercentageFromA = result, "Enter max percentage from A boundary: ");

            ReadFromConsole<int>(result => experiment.N1 = result, "Enter percentage of 1 requirements: ");
            ReadFromConsole<int>(result => experiment.N2 = result, "Enter percentage of 2 requirements: ");
            ReadFromConsole<int>(result => experiment.N12 = result, "Enter percentage of 12 requirements: ");
            ReadFromConsole<int>(result => experiment.N21 = result, "Enter percentage of 21 requirements: ");

            return experiment;
        }

        public static void Main(string[] args)
        {
            var pipeline = new ExperimentPipeline(new ExperimentGenerator());

            var experiment = ReadExperimentFromConsole();
            if (experiment == null) return;

            pipeline.Run(new List<Experiment>() {experiment}).ConfigureAwait(false);

        }
    }
}
