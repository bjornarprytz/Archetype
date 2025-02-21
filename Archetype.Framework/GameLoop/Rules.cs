using Archetype.Framework.Core;
using Archetype.Framework.Events;
using Archetype.Framework.Resolution;
using Archetype.Framework.State;

namespace Archetype.Framework.GameLoop;

public interface  IRules
{
    IGameState CreateInitialState();
    IEnumerable<IEvent> ResolveAction(IGameState state, IScope scope, IActionArgs actionArgs);
    internal Func<IResolutionContext, IEvent> BindEffectResolver(EffectProto effectProto);
    
    // TODO: Effect Resolvers might need to be exposed in a more user readable/friendly way, in order to do rules lookup etc.
}
