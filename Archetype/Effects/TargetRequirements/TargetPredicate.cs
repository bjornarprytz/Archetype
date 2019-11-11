using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public delegate bool TargetPredicate<T>(Unit source, T target) where T : GamePiece;
}
