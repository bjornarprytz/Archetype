using Archetype.Core.Atoms;
using Archetype.Core.Infrastructure;

namespace Archetype.Core.Effects;

public interface IContext<out TSource> : IContext 
    where TSource : IAtom
{
    new TSource Source { get; }
    IAtom IContext.Source => Source;
}
public interface IContext
{
    IGameState GameState { get; }
    ITargetProvider TargetProvider { get; }
    
    IAtom Source { get; }
}
