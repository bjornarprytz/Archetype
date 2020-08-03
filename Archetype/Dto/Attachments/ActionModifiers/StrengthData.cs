namespace Archetype
{
    public class StrengthData : ModifierData
    {
        protected override ActionModifier<Unit> GetModifier()
        {
            return new Strength(Modifier, Multiplier);
        }
    }
}
