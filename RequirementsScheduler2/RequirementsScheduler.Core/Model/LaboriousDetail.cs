using System;

namespace RequirementsScheduler.Core.Model
{
    public class LaboriousDetail : IChainNode, IEquatable<LaboriousDetail>
    {
        public ChainType Type => ChainType.Detail;

        public Detail OnFirst { get; }
        public Detail OnSecond { get; }

        public LaboriousDetail(Detail onFirst, Detail onSecond)
        {
            OnFirst = onFirst;
            OnSecond = onSecond;
        }

        public bool Equals(LaboriousDetail other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(OnFirst, other.OnFirst) && Equals(OnSecond, other.OnSecond);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LaboriousDetail) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((OnFirst != null ? OnFirst.GetHashCode() : 0) * 397) ^ (OnSecond != null ? OnSecond.GetHashCode() : 0);
            }
        }
    }
}
