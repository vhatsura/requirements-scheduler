using System;
using System.Diagnostics;
using RequirementsScheduler.BLL.Service;

namespace RequirementsScheduler.BLL.Model
{
    [DebuggerDisplay("Time: {Time.ToString(\"0.###\")}")]
    public sealed class Downtime : IOnlineChainNode, IEquatable<Downtime>
    {
        public Downtime(double time)
        {
            if (time <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(time), $"Unable to create downtime with {time} value");
            }

            Time = time;
        }

        public double Time { get; }

        public bool Equals(Downtime other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            return ReferenceEquals(this, other) || Time.Equals(other.Time);
        }

        public OnlineChainType Type => OnlineChainType.Downtime;

        public void GenerateP(IRandomizeService randomizeService)
        {
            throw new InvalidOperationException();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj is Downtime downtime && Equals(downtime);
        }

        public override int GetHashCode() => Time.GetHashCode();
    }
}
