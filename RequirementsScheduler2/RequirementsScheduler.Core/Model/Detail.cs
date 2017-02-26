using System.Diagnostics;

namespace RequirementsScheduler.Core.Model
{
    [DebuggerDisplay("A: {Time.A}, B: {Time.B}")]
    public class Detail
    {
        public ProcessingTime Time { get; }

        public Detail(double a, double b)
        {
            Time = new ProcessingTime(a, b);
        }
    }
}
