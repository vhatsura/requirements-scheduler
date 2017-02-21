using System.Collections.Generic;
using RequirementsScheduler.Core.Model;
using RequirementsScheduler.Core.Worker;

namespace RequirementsScheduler.Host.Console
{
    public class Program
    {
        public static Experiment ReadExperimentFromConsole()
        {
            var experiment = new Experiment();

            System.Console.Write("Enter amount of tests: ");
            int value;
            if (!int.TryParse(System.Console.ReadLine(), out value))
            {
                System.Console.Write("Error input");
                return null;
            }

            experiment.TestsAmount = value;

            System.Console.Write("Enter amount of requirements: ");
            if (!int.TryParse(System.Console.ReadLine(), out value))
            {
                System.Console.Write("Error input");
                return null;
            }

            experiment.RequirementsAmount = value;

            System.Console.Write("Enter min boundary range: ");
            if (!int.TryParse(System.Console.ReadLine(), out value))
            {
                System.Console.Write("Error input");
                return null;
            }

            experiment.MinBoundaryRange = value;

            System.Console.Write("Enter max boundary range: ");
            if (!int.TryParse(System.Console.ReadLine(), out value))
            {
                System.Console.Write("Error input");
                return null;
            }

            experiment.MaxBoundaryRange = value;

            System.Console.Write("Enter percentage of 1 requirements: ");
            if (!int.TryParse(System.Console.ReadLine(), out value))
            {
                System.Console.Write("Error input");
                return null;
            }

            experiment.N1 = value;

            System.Console.Write("Enter percentage of 2 requirements: ");
            if (!int.TryParse(System.Console.ReadLine(), out value))
            {
                System.Console.Write("Error input");
                return null;
            }

            experiment.N2 = value;

            System.Console.Write("Enter percentage of 12 requirements: ");
            if (!int.TryParse(System.Console.ReadLine(), out value))
            {
                System.Console.Write("Error input");
                return null;
            }

            experiment.N12 = value;

            System.Console.Write("Enter percentage of 21 requirements: ");
            if (!int.TryParse(System.Console.ReadLine(), out value))
            {
                System.Console.Write("Error input");
                return null;
            }

            experiment.N21 = value;

            return experiment;
        }

        public static void Main(string[] args)
        {
            var pipeline = new ExperimentPipeline();

            var experiment = ReadExperimentFromConsole();
            if (experiment == null) return;

            pipeline.Run(new List<Experiment>() {experiment}).ConfigureAwait(false);

        }
    }
}
