using Archetype.Components.Extensions;
using Archetype.Components.Meta;
using Archetype.Core;
using Archetype.Core.Atoms;
using Archetype.Core.Effects;
using Archetype.Core.Proto.PlayingCard;

namespace Archetype.Components;

internal class ProtoSpell : IProtoSpell
{
    private readonly List<ITargetDescriptor> _targetDescriptors = new ();
    private readonly List<IEffectDescriptor> _effectDescriptors = new ();
        
    private readonly List<IEffect> _effects;
    private List<Func<IContext, IResult>>? _effectFunctions;

    public ProtoSpell(List<IEffect> effects)
    {
        _effects = effects;
    }

    public string Name { get; set; }
    public string ImageUri { get; set; }
    public string SetName { get; set; }
    public CardRarity Rarity { get; set; }
    public CardType Type { get; set; }
    public string SubType { get; set; }
    public string RulesText { get; set; }
    public CardColor Color { get; set; }
    public int Cost { get; set; }
    public int Resources { get; set; }
    public IEnumerable<ITargetDescriptor> TargetDescriptors => _targetDescriptors;
    public IResult Resolve(IContext<ICard> context)
    {
        _effectFunctions ??= CompileEffectFunctions();

        return Result.Aggregate(_effectFunctions.Select(f => f(context)));
    }

    public void Harden()
    {
        // TODO: This logic should be moved to the builder
        
        _effectFunctions ??= CompileEffectFunctions();
          
        _effectDescriptors.Clear();
        _effectDescriptors.AddRange(_effects.Select(effect => effect.ResolveExpression.CreateDescriptor()));
        
        _targetDescriptors.Clear();
        _targetDescriptors.AddRange(
            _effectDescriptors.SelectMany(e => e.GetTargets())
                .Select(t => new TargetDescriptor(t.TargetType))
        );
    }
    
    private List<Func<IContext, IResult>> CompileEffectFunctions()
    {
        return _effects.Select(e => e.ResolveExpression.Compile()).ToList();
    }

    private record TargetDescriptor(Type TargetType) : ITargetDescriptor;
}