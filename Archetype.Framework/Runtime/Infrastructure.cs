using Archetype.Framework.Definitions;
using MediatR;

namespace Archetype.Framework.Runtime;

public interface IDefinitions
{
    IDictionary<string, KeywordDefinition> Keywords { get; set; }
}

public interface IEventHistory
{
    public void Push(IEvent e);
    public IReadOnlyList<IEvent> Events { get; set; }
}

public interface IEffectQueue
{
    void Push(ResolutionContext context);
    IEvent? ResolveNext();
}

public interface IGameLoop
{
    ActionResult Advance();
}

public interface IGameActionHandler
{
    public ActionResult Handle(IRequest args);
}

public class ActionDescription
{
    public string Name { get; set; }
    
    // TODO: Describe requirements of the action
}

public class ActionResult
{
    public IReadOnlyList<ActionDescription> AvailableActions { get; set; }
}