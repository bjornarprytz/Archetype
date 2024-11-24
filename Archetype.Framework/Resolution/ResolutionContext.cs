using Archetype.Framework.Core;
using Archetype.Framework.Events;
using Archetype.Framework.GameLoop;
using Archetype.Framework.Parsing;
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
    
    EffectProto[] GetEffects();
}

public class ResolutionContext : IResolutionContext
{
    public IGameState GetState()
    {
        throw new NotImplementedException();
    }

    public IScope GetScope()
    {
        throw new NotImplementedException();
    }

    public IAtom GetSource()
    {
        throw new NotImplementedException();
    }

    public IAtom? GetTarget(int index)
    {
        throw new NotImplementedException();
    }


    public EffectProto[] GetEffects()
    {
        throw new NotImplementedException();
    }
}