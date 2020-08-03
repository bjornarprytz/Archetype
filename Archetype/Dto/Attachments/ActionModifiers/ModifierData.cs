namespace Archetype
{
    public abstract class ModifierData : ActionParameterData
    {
        public int Modifier { get; set; }
        public float Multiplier { get; set; }

        protected override ActionInfo GetActionInfo(ISource source, ITarget target, GameState gameState)
        {
            return new AttachModifierActionArgs<Unit>(source, target as Unit, GetModifier());
        }

        protected abstract ActionModifier<Unit> GetModifier();
    }
}
