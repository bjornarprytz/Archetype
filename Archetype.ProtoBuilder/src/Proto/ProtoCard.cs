using Archetype.Core.Effects;
using Archetype.Core.Proto;
using Archetype.Core.Triggers;

namespace Archetype.Components.Proto;

internal abstract class ProtoCard : IProtoCard
{
    private readonly List<ITrigger> _triggers = new();
    
    public string Name { set; get; } = "";
    public CardMetaData Meta { get; set; }
    public CardStats Stats { get; set; }


    public IEnumerable<ITrigger> Triggers => _triggers;
    
    public abstract IEnumerable<ITargetDescriptor> TargetDescriptors { get; }
    public abstract IResult Resolve(IContext context);
    public abstract string ContextualRulesText(IContext context);
    
    public void AddTrigger(ITrigger trigger)
    {
        // TODO: Include triggers in the static rules text
        _triggers.Add(trigger);
    }
}