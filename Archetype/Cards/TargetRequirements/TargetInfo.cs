using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class TargetInfo
    {

        public TargetMethod TargetMethod { get; set; }
        public int MinTargets { get; set; }
        public int Count => ChosenTargets.Count;
        public int MaxTargets { get; set; }
        public IEnumerable<GamePiece> Options { get; private set; }

        public bool IsValidTarget(GamePiece target) => Options.Contains(target);
        public List<GamePiece> ChosenTargets { get; private set; }
        public IEnumerable<T> CastTargets<T>() where T : GamePiece => ChosenTargets as IEnumerable<T>;

        public TargetInfo(int min, int max, IEnumerable<GamePiece> options, TargetMethod method)
        {
            MinTargets = min;
            MaxTargets = max;
            Options = options;
            TargetMethod = method;
        }

        public bool Add(GamePiece newTarget)
        {
            if (TargetMethod == TargetMethod.None) return false;
            if (Count >= MaxTargets) return false;
            if (!IsValidTarget(newTarget)) return false;
            if (ChosenTargets.Contains(newTarget)) return false;

            ChosenTargets.Add(newTarget);

            return true;
        }

        public void Clear() => ChosenTargets.Clear();
        public bool Valid => ChosenTargets.Count() <= MaxTargets && ChosenTargets.Count() >= MinTargets;


        public static TargetInfo All(IEnumerable<GamePiece> options)
        {
            var targetInfo = new TargetInfo(0, int.MaxValue, options, TargetMethod.All);

            foreach(var target in options)
            {
                targetInfo.Add(target);
            }

            return targetInfo;
        }

        public static TargetInfo Any(int min, int max, IEnumerable<GamePiece> options) => new TargetInfo(min, max, options, TargetMethod.Any);

        public static TargetInfo Exactly(int n, IEnumerable<GamePiece> options) => new TargetInfo(n, n, options, TargetMethod.Any);

        public static TargetInfo None() => new TargetInfo(0, 0, null, TargetMethod.None);
    }
}