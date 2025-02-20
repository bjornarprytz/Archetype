using Archetype.Framework.Events;
using Archetype.Framework.GameLoop;
using Archetype.Framework.Resolution;
using Archetype.Framework.State;

namespace Archetype.Framework;

public static class Bootstrap
{
    public static IGameRoot Init(IRules rules, IScope? rootScope)
    {
        return new GameRoot(rootScope ?? new Game(), rules);
    }
}

file class GameRoot(IScope rootScope, IRules rules) : IGameRoot
{
    public IScope RootScope => rootScope;
    public IGameState State { get; } = rules.CreateInitialState();

    public IEnumerable<IEvent> TakeAction(IActionArgs actionArgs)
    {
        var scope = RootScope.GetEdgeScope();

        return rules.ResolveAction(scope, actionArgs);
    }
}


file static class Extensions
{
    public static IScope GetEdgeScope(this IScope scope)
    {
        var edgeScope = scope;
        var loopDetector = new HashSet<IScope>(){ edgeScope };
        
        while (edgeScope.CurrentSubScope != null)
        {
            edgeScope = edgeScope.CurrentSubScope;
            
            if (!loopDetector.Add(edgeScope))
            {
                throw new InvalidOperationException("Scope loop detected");
            }
        }

        return edgeScope;
    }
}