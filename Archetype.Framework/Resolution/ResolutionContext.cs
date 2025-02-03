using Archetype.Framework.Core;
using Archetype.Framework.GameLoop;
using Archetype.Framework.State;

namespace Archetype.Framework.Resolution;

public interface IResolutionContext : IValueWhence
{
    [PathPart("state")]
    IGameState GetState();
    
    [PathPart("scope")]
    IScope GetScope();
    
    [PathPart("source")]
    IAtom GetSource();
    
    [PathPart("targets")]
    IAtom? GetTarget(int index);
    
    internal EffectProto[] GetEffects();
}

internal class ResolutionContext(IScope scope, ICard source, IEnumerable<IAtom> targets) : IResolutionContext
{
    private readonly IScope _scope = scope;
    private readonly ICard _source = source;
    private readonly IAtom[] _targets = targets.ToArray();
    
    public IGameState GetState()
    {
        return _scope.State;
    }

    public IScope GetScope()
    {
        return _scope;
    }

    public IAtom GetSource()
    {
        return _source;
    }

    public IAtom? GetTarget(int index)
    {
        return index < _targets.Length ? _targets[index] : null;
    }


    public EffectProto[] GetEffects()
    {
        throw new NotImplementedException();
    }
}