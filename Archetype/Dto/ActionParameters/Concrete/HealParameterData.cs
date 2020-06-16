using System;

namespace Archetype
{
    public class HealParameterData : ActionParameterData
    {
        public ValueDescriptor<int> Strength { get; set; }

        protected override ActionInfo GetActionInfo(Unit source, ITarget target, GameState gameState)
        {
            return new HealActionArgs(source, target as Unit, Strength.CreateGetter(source, gameState));
        }
    }
}
