using System;
using System.Collections.Generic;

namespace RequirementsScheduler.BLL.Model
{
    public abstract class BaseConflict<T> : IEquatable<BaseConflict<T>>
    {
        public IDictionary<int, T> Details { get; } = new Dictionary<int, T>();

        public bool Equals(BaseConflict<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(this, other) || Equals(Details, other.Details);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((BaseConflict<T>) obj);
        }

        public override int GetHashCode()
        {
            return (Details != null ? Details.GetHashCode() : 0);
        }
    }
}