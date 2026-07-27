using System;

namespace Platformer.PCG {
    public sealed class DeterministicRandom {
        readonly Random random;

        public int Seed { get; }

        public DeterministicRandom(int seed) {
            Seed = seed;
            random = new Random(seed);
        }

        public int Range(int minimumInclusive, int maximumExclusive) =>
            random.Next(minimumInclusive, maximumExclusive);

        public double Value() => random.NextDouble();
    }
}
