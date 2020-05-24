namespace Archetype
{
    public class DrawInfo : ActionInfo
    {
        public DrawInfo(Unit source, int strength) : base(source, strength)
        {
            Strength = source.SourceModifier.Get<DrawInfo>();
        }
    }
}
