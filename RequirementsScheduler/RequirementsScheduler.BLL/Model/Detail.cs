using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace RequirementsScheduler.BLL.Model
{
    [DebuggerDisplay("Number: {Number} A: {Time.A.ToString(\"0.###\")} B: {Time.B.ToString(\"0.###\")}")]
    public class Detail : IOnlineChainNode
    {
        public ProcessingTime Time { get; }

        public int Number { get; }

        public Detail(double a, double b, int number)
            : this(new ProcessingTime(a, b), number)
        {

        }

        [JsonConstructor]
        public Detail(ProcessingTime time, int number)
        {
            if (number <= 0)
                throw new ArgumentOutOfRangeException(nameof(number));

            Time = time;
            Number = number;
        }

        public OnlineChainType Type => OnlineChainType.Detail;
    }
}
