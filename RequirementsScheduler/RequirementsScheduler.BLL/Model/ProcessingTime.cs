using System;
using Newtonsoft.Json;
using RequirementsScheduler.BLL.Service;
using RequirementsScheduler.DAL.Model;

namespace RequirementsScheduler.BLL.Model
{
    public class ProcessingTime : IEquatable<ProcessingTime>
    {
        [JsonConstructor]
        public ProcessingTime(double a, double b, Distribution distribution)
        {
            if (a > b || a <= 0 || b <= 0)
                throw new ArgumentOutOfRangeException();

            A = a;
            B = b;
            Distribution = distribution;
        }

        public double A { get; }
        public double B { get; }
        public Distribution Distribution { get; }

        public double P { get; private set; }

        public double Average => (B + A) / 2;

        public bool Equals(ProcessingTime other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return A.Equals(other.A) && B.Equals(other.B);
        }

        public void GenerateP()
        {
            P = RandomizeService.GetRandomDouble(A, B, Distribution);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ProcessingTime) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (A.GetHashCode() * 397) ^ B.GetHashCode();
            }
        }

        public override string ToString() => $"A: {A:0.###} B: {B:0.###} P: {P:0.###}";
    }
}