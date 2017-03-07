using System;
using System.Diagnostics;

namespace RequirementsScheduler.Core.Model
{
    [DebuggerDisplay("Number: {Number} A: {Time.A.ToString(\"0.###\")} B: {Time.B.ToString(\"0.###\")}")]
    public class Detail
    {
        public ProcessingTime Time { get; }

        public int Number { get; }

        public Detail(double a, double b, int number)
            : this(new ProcessingTime(a, b), number)
        {

        }

        public Detail(ProcessingTime time, int number)
        {
            if (number <= 0)
                throw new ArgumentOutOfRangeException(nameof(number));

            Time = time;
            Number = number;
        }
    }
}
