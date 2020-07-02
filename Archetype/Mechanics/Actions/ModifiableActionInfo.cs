using System;

namespace Archetype
{
    public abstract class ModifiableActionInfo : ActionInfo
    {
        private Func<int> _getter { get; set; }
        public int Strength => (int) ((_getter() + Modifier) * Multiplier);
        public int Modifier { get; set; } = 0;
        public float Multiplier { get; set; } = 1;
        public ModifiableActionInfo(ISource source, ITarget target, Func<int> getter) : base(source, target)
        {
            _getter = getter;
        }
    }
}
