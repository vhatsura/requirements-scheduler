using System;
using System.Collections.Generic;

namespace RequirementsScheduler.BLL.Model
{
    public abstract class BaseConflict<T> : IEquatable<BaseConflict<T>>
    {
        public List<T> Details { get; } = new List<T>();

        public bool Equals(BaseConflict<T> other)
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
            return Equals((BaseConflict<T>)obj);
        }

        public override int GetHashCode()
        {
            return (Details != null ? Details.GetHashCode() : 0);
        }
    }
}
