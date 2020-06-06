using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class TargetInfo
    {

        public SelectionMethod TargetMethod { get; set; }
        public int MinTargets { get; set; }
        public int Count => ChosenTargets.Count;
        public int MaxTargets { get; set; }
        public IEnumerable<ITarget> Options { get; private set; }

        public bool IsValidTarget(ITarget target) => Options.Contains(target);
        public List<ITarget> ChosenTargets { get; private set; }
        public IEnumerable<T> CastTargets<T>() where T : ITarget => ChosenTargets as IEnumerable<T>;

        public TargetInfo(int min, int max, IEnumerable<ITarget> options, SelectionMethod method)
        {
            MinTargets = min;
            MaxTargets = max;
            Options = options;
            TargetMethod = method;
        }

        public bool Add(ITarget newTarget)
        {
            if (TargetMethod == SelectionMethod.None) return false;
            if (Count >= MaxTargets) return false;
            if (!IsValidTarget(newTarget)) return false;
            if (ChosenTargets.Contains(newTarget)) return false;

            ChosenTargets.Add(newTarget);

            return true;
        }

        public void Clear() => ChosenTargets.Clear();
        public bool Valid => ChosenTargets.Count() <= MaxTargets && ChosenTargets.Count() >= MinTargets;


        public static TargetInfo Any(int min, int max, IEnumerable<ITarget> options) => new TargetInfo(min, max, options, SelectionMethod.Any);

        public static TargetInfo Exactly(int n, IEnumerable<ITarget> options) => new TargetInfo(n, n, options, SelectionMethod.Any);

        public static TargetInfo None() => new TargetInfo(0, 0, null, SelectionMethod.None);

        public static TargetInfo All(IEnumerable<ITarget> options)
        {
            var targetInfo = new TargetInfo(0, int.MaxValue, options, SelectionMethod.All);

            foreach(var target in options)
            {
                targetInfo.Add(target);
            }

            return targetInfo;
        }
        public static TargetInfo Random(int n, IEnumerable<ITarget> options)
        {
            var targetInfo = new TargetInfo(0, n, options, SelectionMethod.Random);

            foreach(var target in options.GrabRandom(n))
            {
                targetInfo.Add(target);
            }

            return targetInfo;
        }
    }
}