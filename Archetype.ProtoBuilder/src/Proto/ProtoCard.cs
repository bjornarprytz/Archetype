using Archetype.Core.Atoms.Cards;
using Archetype.Core.Effects;
using Archetype.Core.Proto;

namespace Archetype.Components.Proto;

internal abstract class ProtoCard : IProtoCard
{
    public string Name { set; get;  }
    public CardMetaData Meta { get; set; }
    public CardStats Stats { get; set; }
    
    public abstract IEnumerable<ITargetDescriptor> TargetDescriptors { get; }
    public abstract IResult Resolve(IContext context);
    public abstract string ContextualRulesText(IContext<ICard> context);
}