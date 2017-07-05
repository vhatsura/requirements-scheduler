using System;
using Newtonsoft.Json;
using RequirementsScheduler.BLL.Service;

namespace RequirementsScheduler.BLL.Model
{
    public class ProcessingTime : IEquatable<ProcessingTime>
    {
        public double A { get; }
        public double B { get; }

        public double P { get; private set; }

        public double Average => (B + A) / 2;

        [JsonConstructor]
        public ProcessingTime(double a, double b)
        {
            if(a > b || a <= 0  || b <= 0)
                throw new ArgumentOutOfRangeException();

            A = a;
            B = b;
        }

        public void GenerateP()
        {
            P = RandomizeService.GetRandomDouble(A, B);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ProcessingTime) obj);
        }

        public bool Equals(ProcessingTime other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return A.Equals(other.A) && B.Equals(other.B);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (A.GetHashCode() * 397) ^ B.GetHashCode();
            }
        }

        public override string ToString()
        {
            return $"A: {A:0.###} B: {B:0.###} P: {P:0.###}";
        }
    }
}
