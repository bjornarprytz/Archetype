using Archetype.Core.Atoms.Cards;
using Archetype.Core.Effects;
using Archetype.Core.Proto.PlayingCard;

namespace Archetype.Components.Protos;

internal abstract class ProtoCard : IProtoPlayingCard
{
    
    public string Name => Meta.Name;
    public CardMetaData Meta { get; set; }
    public CardStats Stats { get; set; }
    
    public abstract IEnumerable<ITargetDescriptor> TargetDescriptors { get; }
    public abstract IResult Resolve(IContext context);
    public abstract string ContextualRulesText(IContext<ICard> context);
}