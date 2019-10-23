using System;
using System.Diagnostics;

namespace RequirementsScheduler.BLL.Model
{
    [DebuggerDisplay("Time: {Time.ToString(\"0.###\")}")]
    public sealed class Downtime : IOnlineChainNode, IEquatable<Downtime>
    {
        public double Time { get; }

        public Downtime(double time)
        {
            if (time <= 0)
                throw new ArgumentOutOfRangeException(nameof(time), $"Unable to create downtime with {time} value");

            Time = time;
        }

        public OnlineChainType Type => OnlineChainType.Downtime;
        public void GenerateP()
        {
            throw new InvalidOperationException();
        }

        public bool Equals(Downtime other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Time.Equals(other.Time);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Downtime && Equals((Downtime) obj);
        }

        public override int GetHashCode()
        {
            return Time.GetHashCode();
        }
    }
}
