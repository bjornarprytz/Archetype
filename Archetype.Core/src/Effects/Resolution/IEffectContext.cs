using Archetype.Core.Atoms;

namespace Archetype.Core.Effects;

public interface IEffectContext
{
    IGameState GameState { get; }
    IAtom Source { get; }
    IEffectProvider EffectProvider { get; }
    ITargetProvider TargetProvider { get; }
}

public interface IEffectProvider
{
    public IEnumerable<IEffect> Effects { get; }
}

public interface ITargetProvider
{
    T GetTarget<T>() where T : IAtom;
    T GetTarget<T>(int index) where T : IAtom;
}