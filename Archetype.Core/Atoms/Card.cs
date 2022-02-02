using Archetype.Core.Atoms.Base;
using Archetype.Core.Play;
using Archetype.Core.Proto;
using Archetype.View.Atoms;
using Archetype.View.Atoms.MetaData;
using Archetype.View.Infrastructure;
using Archetype.View.Infrastructure.Play;

namespace Archetype.Core.Atoms;

public interface ICard : 
    IPiece<ICard>, 
    ICardFront, 
    IEffectProvider
{
    IEffectResult<ICard, int> ReduceCost(int x);
}

internal class Card : Piece<ICard>, ICard
{
    private readonly List<ITargetDescriptor> _targetDescriptors;
    private readonly List<IEffectDescriptor> _effectDescriptors;
    private readonly List<IEffect> _effects;

    public Card(ICardProtoData protoData) : base(protoData.Name)
    {
        _targetDescriptors = protoData.TargetDescriptors.ToList();
        _effectDescriptors = protoData.EffectDescriptors.ToList();
        _effects = protoData.Effects.ToList();
        MetaData = protoData.MetaData;
        Cost = protoData.Cost;
        Range = protoData.Range;
    }

    public int Cost { get; private set; }
    public int Range { get; private set; }
    public IEnumerable<ITargetDescriptor> TargetDescriptors => _targetDescriptors;
    public IEnumerable<IEffectDescriptor> EffectDescriptors => _effectDescriptors;


    public CardMetaData MetaData { get; }
    public IEnumerable<IEffect> Effects => _effects;
        
        
    public IEffectResult<ICard, int> ReduceCost(int x)
    {
        Console.WriteLine($"Reducing cost by {x}!");

        Cost -= x;

        return ResultFactory.Create(this, x);
    }

    protected override ICard Self => this;
}