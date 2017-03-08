using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Moq;
using RequirementsScheduler.BLL.Model;
using RequirementsScheduler.BLL.Service;
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
            var pipeline = new ExperimentPipeline(new ExperimentGenerator(), Mock.Of<IWorkerExperimentService>());

            var experiment = ReadExperimentFromConsole();
            if (experiment == null) return;

            var stopwatch = Stopwatch.StartNew();
            pipeline.Run(new List<Experiment>() {experiment}).ConfigureAwait(false);
            stopwatch.Stop();

            System.Console.ForegroundColor = ConsoleColor.DarkGreen;
            System.Console.WriteLine("\n\n---------   RESULTS OF EXPERIMENTS' EXECUTION   ---------");

            System.Console.WriteLine($"Time of execution experiments: {TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds):%m' minute(s) '%s' second(s)'}\n\n");

            System.Console.WriteLine($"Amounts of tests: {experiment.TestsAmount}");
            var stop11Count = experiment.Results.Count(result => result.Result.Type == ResultType.STOP1_1);
            var stop12Count = experiment.Results.Count(result => result.Result.Type == ResultType.STOP1_2);
            var stop13Count = experiment.Results.Count(result => result.Result.Type == ResultType.STOP1_3);
            var stop14Count = experiment.Results.Count(result => result.Result.Type == ResultType.STOP1_4);
            System.Console.WriteLine($"Amounts of resolved tests in offline mode: {experiment.Results.Count}, {(experiment.Results.Count * 100 / (double)experiment.TestsAmount):0.###}%");
            System.Console.WriteLine($"Amounts of resolved tests in STOP 1.1: {stop11Count}, {(stop11Count * 100 / (double)experiment.TestsAmount):0.###}%");
            System.Console.WriteLine($"Amounts of resolved tests in STOP 1.2: {stop12Count}, {(stop12Count * 100 / (double)experiment.TestsAmount):###}%");
            System.Console.WriteLine($"Amounts of resolved tests in STOP 1.3: {stop13Count}, {(stop13Count * 100 / (double)experiment.TestsAmount):0.###}%");
            System.Console.WriteLine($"Amounts of resolved tests in STOP 1.4: {stop14Count}, {(stop14Count * 100 / (double)experiment.TestsAmount):0.###}%");

            System.Console.ReadKey();

        }
    }
}
