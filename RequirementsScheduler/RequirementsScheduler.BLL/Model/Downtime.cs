﻿using System;

namespace RequirementsScheduler.BLL.Model
{
    public sealed class Downtime : IOnlineChainNode, IEquatable<Downtime>
    {
        public double Time { get; }

        public Downtime(double time)
        {
            if (time <= 0)
                throw new ArgumentOutOfRangeException();

            Time = time;
        }

        public OnlineChainType Type => OnlineChainType.Downtime;

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