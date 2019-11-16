using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class TargetParams<T> where T : GamePiece
    {
        private int _number;
        private TargetPredicate<T> _predicate;

        public TargetParams(int n, TargetPredicate<T> predicate)
        {
            _number = n;
            _predicate = predicate;
        }

        public static TargetParams<T> Enemy(int n)
        {
            return new TargetParams<T>(n, (s, t) => s.EnemyOf(t));
        }
        public static TargetParams<T> Ally(int n)
        {
            return new TargetParams<T>(n, (s, t) => s.AllyOf(t));
        }

        internal Choose<T> GetPrompt(Unit owner, IEnumerable<T> possibleChoices)
        {
            return new Choose<T>(_number, possibleChoices.Where((p => _predicate(owner, p))));
        }
    }
}
