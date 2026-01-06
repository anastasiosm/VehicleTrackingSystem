using System;
using System.Threading; // Add this for ThreadLocal

namespace VehicleTracking.Infrastructure.Services
{
    /// <summary>
    /// Thread-safe random number generator.
    /// Uses ThreadLocal to ensure each thread gets its own Random instance with a unique seed.
    /// </summary>
    /// <remarks>
    /// Creating multiple Random instances in quick succession can result in identical sequences
    /// because they may be initialized with the same seed (based on system clock).
    /// This class solves that problem by using ThreadLocal storage.
    /// </remarks>
    public static class ThreadSafeRandom
    {
        private static readonly ThreadLocal<Random> _random = new ThreadLocal<Random>(() =>
        {
            // Use a combination of thread ID and ticks for better seed distribution
            return new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId));
        });

        /// <summary>
        /// Returns a non-negative random integer.
        /// </summary>
        public static int Next()
        {
            return _random.Value.Next();
        }

        /// <summary>
        /// Returns a non-negative random integer that is less than the specified maximum.
        /// </summary>
        public static int Next(int maxValue)
        {
            return _random.Value.Next(maxValue);
        }

        /// <summary>
        /// Returns a random integer that is within a specified range.
        /// </summary>
        public static int Next(int minValue, int maxValue)
        {
            return _random.Value.Next(minValue, maxValue);
        }

        /// <summary>
        /// Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        public static double NextDouble()
        {
            return _random.Value.NextDouble();
        }
    }
}
