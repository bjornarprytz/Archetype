using Archetype.Framework.Core;
using Archetype.Framework.Events;
using Archetype.Framework.State;

namespace Archetype.Framework.Resolution;

public interface IResolutionContext
{
    IGameState GetGameState();
    IEventHistory GetEventHistory();
    int? GetVariable(string name);
    IAtom GetSource();
    
    EffectResolver[] GetEffects();
}

public class ResolutionContext : IResolutionContext
{
    public IGameState GetGameState()
    {
        throw new NotImplementedException();
    }

    public IEventHistory GetEventHistory()
    {
        throw new NotImplementedException();
    }

    public int? GetVariable(string name)
    {
        throw new NotImplementedException();
    }

    public IAtom GetSource()
    {
        throw new NotImplementedException();
    }

    public EffectResolver[] GetEffects()
    {
        throw new NotImplementedException();
    }
}