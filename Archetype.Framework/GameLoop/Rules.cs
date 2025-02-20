using Archetype.Framework.Events;
using Archetype.Framework.Resolution;
using Archetype.Framework.State;

namespace Archetype.Framework.GameLoop;

public interface IRules
{
    IGameState CreateInitialState();
    IEnumerable<IEvent> ResolveAction(IScope scope, IActionArgs actionArgs);
}

public interface IKeyword
{
    public string Keyword { get; }
}
