using System;

namespace Archetype
{
    public static class ModifierDataMapperExtension
    {
        public static ActionModifier<Unit> GetModifier(this ModifiableActionType modifierType, ModifierStance stance, int modifier, float multiplier)
        {
            throw new NotImplementedException();

            return (stance, modifierType) switch
            {
                (ModifierStance.Offensive, ModifiableActionType.Damage) => new OffensiveActionModifier<Unit, DamageActionArgs>(modifier, multiplier),
                (ModifierStance.Defensive, ModifiableActionType.Damage) => new OffensiveActionModifier<Unit, DamageActionArgs>(modifier, multiplier),
                (ModifierStance.Offensive, ModifiableActionType.Discard) => new OffensiveActionModifier<Unit, DiscardActionArgs>(modifier, multiplier),
                (ModifierStance.Defensive, ModifiableActionType.Discard) => new OffensiveActionModifier<Unit, DamageActionArgs>(modifier, multiplier),
                // TODO: reconsider this
            };
        }
    }
}
