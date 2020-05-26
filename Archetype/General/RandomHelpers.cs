using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Archetype
{
    public static class RandomHelpers
    {
        public static IEnumerable<T> GrabRandom<T>(this IEnumerable<T> candidates, int n, Random rand=null)
        {
            if (n >= candidates.Count()) return candidates;

            if (rand == null) rand = new Random();

            return candidates.OrderBy(i => rand.Next()).Take(n);
        }

        public static T WeightedChoice<T>(this IEnumerable<(T, int)> candidates, Random rand=null)
        {
            if (candidates.Count() == 0) throw new ArgumentException("Can't make a weighted choice from an empty collection.");

            int sum = candidates.Select(t => t.Item2).Sum();

            if (rand == null) rand = new Random();

            int number = rand.Next(sum);

            foreach (var candidate in candidates)
            {
                number -= candidate.Item2;

                if (number <= 0) return candidate.Item1;
            }

            return candidates.Last().Item1;
        }
    }
}
