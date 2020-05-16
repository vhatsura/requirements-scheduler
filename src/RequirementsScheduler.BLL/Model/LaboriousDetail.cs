using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace RequirementsScheduler.BLL.Model
{
    [DebuggerDisplay("Number: {" + nameof(Number) + "}, OnFirst: {" + nameof(OnFirst) + "}, OnSecond: {" +
                     nameof(OnSecond) + "}")]
    public class LaboriousDetail : IChainNode, IEquatable<LaboriousDetail>
    {
        [JsonConstructor]
        public LaboriousDetail(Detail onFirst, Detail onSecond, int number)
        {
            Number = number;

            OnFirst = new Detail(onFirst);
            OnSecond = new Detail(onSecond);
        }

        public LaboriousDetail(ProcessingTime onFirst, ProcessingTime onSecond, int number)
        {
            Number = number;

            OnFirst = new Detail(onFirst.A, onFirst.B, onFirst.Distribution, Number);
            OnSecond = new Detail(onSecond.A, onSecond.B, onSecond.Distribution, Number);
        }

        public int Number { get; }

        public Detail OnFirst { get; }
        public Detail OnSecond { get; }
        public ChainType Type => ChainType.Detail;

        public bool Equals(LaboriousDetail other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(OnFirst, other.OnFirst) && Equals(OnSecond, other.OnSecond);
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

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((LaboriousDetail) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((OnFirst != null ? OnFirst.GetHashCode() : 0) * 397) ^
                       (OnSecond != null ? OnSecond.GetHashCode() : 0);
            }
        }
    }
}
