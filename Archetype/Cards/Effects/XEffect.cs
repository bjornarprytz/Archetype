using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public abstract class XEffect : Effect
    {
        public int X { get { return _x > 0 ? _x : 0; } set { _x = value; } }

        private int _x;

        public XEffect(Unit source, int x) : base(source)
        {
            X = x;
        }

        public void ModEffect(List<Modifier> modifiers)
        {
            modifiers.ForEach(m => X += m.Value);
        }

        public int ModOutput(List<Modifier> modifiers)
        {
            int modifiedValue = _x;

            modifiers.ForEach(m => modifiedValue += m.Value);

            return modifiedValue > 0 ? modifiedValue : 0;
        }

    }
}
