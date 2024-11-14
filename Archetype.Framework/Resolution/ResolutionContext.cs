using Archetype.Framework.Core;
using Archetype.Framework.Events;
using Archetype.Framework.GameLoop;
using Archetype.Framework.State;

namespace Archetype.Framework.Resolution;

public interface IResolutionContext
{
    IScope GetScope();
    
    IAtom GetSource();
    
    EffectResolver[] GetEffects();
}

public class ResolutionContext : IResolutionContext
{
    public IScope GetScope()
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