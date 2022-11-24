using Archetype.Components.Extensions;
using Archetype.Components.Meta;
using Archetype.Core;
using Archetype.Core.Atoms;
using Archetype.Core.Effects;
using Archetype.Core.Extensions;
using Archetype.Core.Proto.PlayingCard;

namespace Archetype.Components;

internal class ProtoSpell : IProtoSpell
{
    private readonly List<ITargetDescriptor> _targetDescriptors = new (); 
    private readonly List<IEffectDescriptor> _effectDescriptors = new (); 
        
    private readonly List<IEffect> _effects;

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
        throw new NotImplementedException();
    }
   
    public void GenerateDescriptors()
    {
        // TODO: Check if this actually works
        
        _targetDescriptors.Clear();
        _effectDescriptors.Clear();
            
        _effectDescriptors.AddRange(_effects.Select(effect => effect.ResolveExpression.CreateDescriptor()));

        var targets =
            _effectDescriptors.Select(descriptor => descriptor.Affected.Description.Value)
                .OfType<ITargetProperty>().ToList();

        targets.AddRange(_effectDescriptors.SelectMany(descriptor => descriptor.Operands)
            .Select(operand => operand.Value.Value).OfType<ITargetProperty>());

        var targetDescriptors = new Dictionary<Type, Dictionary<int, ITargetProperty>>();

        foreach (var target in targets)
        {
            targetDescriptors.GetOrSet(target.TargetType)[target.TargetIndex] = target;
        }

        foreach (var target in targets)
        {
            var targetsOfType = targetDescriptors[target.TargetType]; 
                
            if (!targetsOfType.ContainsKey(target.TargetIndex))
            {
                continue;
            }
                
            _targetDescriptors.Add(new TargetDescriptor(target.TargetType));
            targetsOfType.Remove(target.TargetIndex);
        }
    }

    private record TargetDescriptor(Type TargetType) : ITargetDescriptor;
}