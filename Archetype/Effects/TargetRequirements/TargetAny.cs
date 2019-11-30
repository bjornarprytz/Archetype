using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class TargetAny<T> : TargetParams<T> where T : GamePiece
    {
        private int _number;

        public TargetAny(int n, TargetPredicate<T> predicate) 
            : base (predicate)
        {
            _number = n;
        }

        public static TargetAny<T> Enemy(int n)
        {
            return new TargetAny<T>(n, (s, t) => s.EnemyOf(t));
        }
        public static TargetAny<T> Ally(int n)
        {
            return new TargetAny<T>(n, (s, t) => s.AllyOf(t));
        }

        internal override PromptResponse GetTargets(Unit owner, IEnumerable<T> options, RequiredAction actionPrompt)
        {
            return actionPrompt(new Choose<T>(_number, options.Where(o => _predicate(owner, o))));
        }
    }
}
