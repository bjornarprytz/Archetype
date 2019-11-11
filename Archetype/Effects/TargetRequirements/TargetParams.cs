using System;
using System.Collections.Generic;
using System.Text;

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

        internal ChooseTargets<T> GetPrompt(Unit owner)
        {
            return new ChooseTargets<T>(_number, (t) => _predicate(owner, t));
        }
    }
}
