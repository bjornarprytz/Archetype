using Archetype.Core.Atoms;
using Archetype.Core.Atoms.Base;
using Archetype.Core.Infrastructure;

namespace Archetype.Core.Play;

public interface IContext<out TSource> : IContext
    where TSource : IGameAtom
{
    new TSource Source { get; }
}
    
public interface IContext : IDisposable
{
    IGameState GameState { get; }
    IHistoryReader History { get; }
    IGameAtom Source { get; }
    IMapNode Whence { get; }
    IEffectProvider EffectProvider { get; }
    ITargetProvider TargetProvider { get; }
}