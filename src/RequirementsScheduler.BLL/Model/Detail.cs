using System;
using System.Diagnostics;
using Newtonsoft.Json;
using RequirementsScheduler.BLL.Service;
using RequirementsScheduler.DAL.Model;

namespace RequirementsScheduler.BLL.Model
{
    [DebuggerDisplay("#{Number} - ({Time})")]
    public class Detail : IOnlineChainNode
    {
        public Detail(Detail detail)
            : this(detail.Time, detail.Number)
        {
        }

        public Detail(double a, double b, Distribution distribution, int number)
            : this(new ProcessingTime(a, b, distribution), number)
        {
        }

        [JsonConstructor]
        public Detail(ProcessingTime time, int number)
        {
            if (number <= 0) throw new ArgumentOutOfRangeException(nameof(number));

            Time = time;
            Number = number;
        }

        public ProcessingTime Time { get; }

        public int Number { get; }

        public OnlineChainType Type => OnlineChainType.Detail;

        public void GenerateP(IRandomizeService randomizeService)
        {
            Time.GenerateP(randomizeService);
        }

        public override string ToString() => $"Number: {Number} Time: ({Time})";
    }
}