namespace Archetype.Core.Play;

public interface IEffectProvider
{
    IEnumerable<IEffect> Effects { get; }
}