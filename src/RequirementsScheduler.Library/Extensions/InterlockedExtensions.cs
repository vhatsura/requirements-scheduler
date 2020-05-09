using System;
using System.Threading;

namespace RequirementsScheduler.Library.Extensions
{
    public class InterlockedExtensions
    {
        public static void Add(ref double location1, double value)
        {
            var newCurrentValue = location1; // non-volatile read, so may be stale
            while (true)
            {
                var currentValue = newCurrentValue;
                var newValue = currentValue + value;
                newCurrentValue = Interlocked.CompareExchange(ref location1, newValue, currentValue);
                if (newCurrentValue == currentValue)
                {
                    return;
                }
            }
        }

        public static void Max(ref float location, float value)
        {
            float initialValue, newValue;
            do
            {
                initialValue = location;
                newValue = Math.Max(initialValue, value);
            } while (Interlocked.CompareExchange(ref location, newValue,
                initialValue) != initialValue);
        }
    }
}
