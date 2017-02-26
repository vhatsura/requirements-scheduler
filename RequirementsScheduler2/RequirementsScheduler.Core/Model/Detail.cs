using System.Diagnostics;

namespace RequirementsScheduler.Core.Model
{
    [DebuggerDisplay("A: {Time.A.ToString(\"0.###\")} B: {Time.B.ToString(\"0.###\")}")]
    public class Detail
    {
        public ProcessingTime Time { get; }

        public Detail(double a, double b)
        {
            Time = new ProcessingTime(a, b);
        }
    }
}
