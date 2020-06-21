using System;

namespace Archetype
{
    public abstract class ParameterizedActionInfo<T> : ActionInfo
    {
        private Func<T> _getter { get; set; }
        public T Strength => _getter();
        public ParameterizedActionInfo(ISource source, ITarget target, Func<T> getter) : base(source, target)
        {
            _getter = getter;
        }
    }
}
