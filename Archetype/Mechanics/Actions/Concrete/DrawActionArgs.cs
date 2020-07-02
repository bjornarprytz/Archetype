using System;

namespace Archetype
{
    public class DrawActionArgs : ModifiableActionInfo
    {
        public DrawActionArgs(Unit source, Unit target, Func<int> getter) : base(source, target, getter) { }

        protected override void Resolve()
        {
            (Target as Unit).Draw(Strength);
        }
    }
}
