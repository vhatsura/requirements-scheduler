using System;
using System.Collections.Generic;

namespace RequirementsScheduler.BLL.Model
{
    public class Conflict : IChainNode, IEquatable<Conflict>
    {
        public ChainType Type => ChainType.Conflict;

        public List<LaboriousDetail> Details { get; } = new List<LaboriousDetail>();

        public bool Equals(Conflict other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Details, other.Details);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Conflict) obj);
        }

        public override int GetHashCode()
        {
            return (Details != null ? Details.GetHashCode() : 0);
        }
    }
}
