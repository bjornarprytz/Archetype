﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class TargetAny<T> : TargetParams<T> where T : GamePiece
    {
        private int _number;

        public override int Max => _number;

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
    }
}