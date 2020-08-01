using System;

namespace Archetype
{
    public class HealParameterData : ActionParameterData
    {
        public ValueDescriptor<int> Strength { get; set; }

        protected override ActionInfo GetActionInfo(ISource source, ITarget target, GameState gameState)
        {
            return new HealActionArgs(source as Unit, target as Unit, Strength.CreateGetter(source as Unit, gameState));
        }
    }
}
