using System.Reflection;
using Archetype.Framework.Core;
using Archetype.Framework.Events;
using Archetype.Framework.Resolution;
using Archetype.Framework.State;

namespace Archetype.Framework.GameLoop;

public interface IRules
{
    bool ValidateAction(IGameState? state, IScope scope, IActionArgs actionArgs, out string error);
    
    bool TryBindContext(IGameState state, IScope scope, IActionArgs actionArgs, out IResolutionContext? resolutionContext);
    IEnumerable<IEvent> ResolveEffect(IResolutionContext context, EffectProto effectProto);
}

public interface IKeyword
{
    public string Keyword { get; }
}
