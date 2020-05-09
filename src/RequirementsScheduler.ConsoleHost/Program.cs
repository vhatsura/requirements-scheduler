using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RequirementsScheduler.BLL.Model;
using RequirementsScheduler.BLL.Service;
using RequirementsScheduler.DAL;
using RequirementsScheduler.Library.Worker;

namespace RequirementsScheduler.ConsoleHost
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
                result = default;
                return false;
            }
        }

        private static void ReadFromConsole<T>(Action<T> setter, string message)
        {
            System.Console.Write(message);
            var line = System.Console.ReadLine();

            if (!ConvertValue(line, out T value))
            {
                System.Console.Write("Error input");
                return;
            }

            setter.Invoke(value);
        }

        public static Experiment ReadExperimentFromConsole()
        {
            var experiment = new Experiment
            {
                Id = Guid.NewGuid()
            };

            ReadFromConsole<int>(result => experiment.TestsAmount = result, "Enter amount of tests: ");
            ReadFromConsole<int>(result => experiment.RequirementsAmount = result, "Enter amount of requirements: ");

            ReadFromConsole<int>(result => experiment.MinBoundaryRange = result, "Enter min boundary range: ");
            ReadFromConsole<int>(result => experiment.MaxBoundaryRange = result, "Enter max boundary range: ");

            ReadFromConsole<int>(result => experiment.MinPercentageFromA = result,
                "Enter min percentage from A boundary: ");
            ReadFromConsole<int>(result => experiment.MaxPercentageFromA = result,
                "Enter max percentage from A boundary: ");

            ReadFromConsole<int>(result => experiment.N1 = result, "Enter percentage of 1 requirements: ");
            ReadFromConsole<int>(result => experiment.N2 = result, "Enter percentage of 2 requirements: ");
            ReadFromConsole<int>(result => experiment.N12 = result, "Enter percentage of 12 requirements: ");
            ReadFromConsole<int>(result => experiment.N21 = result, "Enter percentage of 21 requirements: ");

            return experiment;
        }

        public static void Main(string[] args)
        {
            var experiment = ReadExperimentFromConsole();
            if (experiment == null) return;

            var reportsServiceMock = new Mock<IReportsService>();
            ExperimentReport report = null;
            reportsServiceMock
                .Setup(r => r.Save(It.Is<ExperimentReport>(e => e.ExperimentId == experiment.Id)))
                .Callback<ExperimentReport>(rep => report = rep);

            var pipeline = new ExperimentPipeline(
                new ExperimentGenerator(new RandomizeService()),
                Mock.Of<IWorkerExperimentService>(),
                new ExperimentTestResultFileService(),
                reportsServiceMock.Object,
                Mock.Of<ILogger<ExperimentPipeline>>(),
                Mock.Of<IOptions<DbSettings>>(),
                new OnlineExecutor());

            var stopwatch = Stopwatch.StartNew();
            pipeline.Run(new List<Experiment> {experiment}).ConfigureAwait(false);
            stopwatch.Stop();

            System.Console.ForegroundColor = ConsoleColor.DarkGreen;
            System.Console.WriteLine("\n\n---------   RESULTS OF EXPERIMENTS' EXECUTION   ---------");

            System.Console.WriteLine(
                $"Time of execution experiments: {TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds):%m' minute(s) '%s' second(s)'}\n\n");

            System.Console.WriteLine($"Amounts of tests: {experiment.TestsAmount}");
            System.Console.WriteLine(
                $"Amounts of resolved tests in offline mode: {experiment.Results.Count}, {experiment.Results.Count * 100 / (double) experiment.TestsAmount:0.###}%");
            System.Console.WriteLine($"Amounts of resolved tests in STOP 1.1: {report?.Stop1Percentage}%");
            System.Console.WriteLine($"Amounts of resolved tests in STOP 1.2: {report?.Stop2Percentage}%");
            System.Console.WriteLine($"Amounts of resolved tests in STOP 1.3: {report?.Stop3Percentage}%");
            System.Console.WriteLine($"Amounts of resolved tests in STOP 1.4: {report?.Stop4Percentage}%");

            System.Console.ReadKey();
        }
    }
}
