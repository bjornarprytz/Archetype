﻿using System.Text;
using Archetype.Components.Extensions;
using Archetype.Components.Meta;
using Archetype.Core.Atoms;
using Archetype.Core.Atoms.Cards;
using Archetype.Core.Effects;
using Archetype.Core.Proto.PlayingCard;

namespace Archetype.Components.Protos;

internal class ProtoSpell : ProtoCard, IProtoSpell
{
    private readonly List<IEffect> _effects = new ();
    private readonly List<IEffectDescriptor> _effectDescriptors = new ();
    private readonly Dictionary<int, ITargetDescriptor> _targetDescriptors = new ();
        
    private readonly List<Func<IContext<ICard>, IResult>> _effectFunctions = new ();

    public override IEnumerable<ITargetDescriptor> TargetDescriptors => _targetDescriptors.OrderBy(t => t.Key).Select(t => t.Value);
    public override IResult Resolve(IContext<ICard> context)
    {
        return Result.Aggregate(_effectFunctions.Select(f => f(context)));
        
        // TODO: Move the card to the graveyard
    }

    public override string ContextualRulesText(IContext<ICard> context)
    {
        // [keyword], {parameterIndex}, <targetIndex>
        
        var sb = new StringBuilder();

        foreach (var effectText in _effectDescriptors.Select(e => e.GetDynamicRulesText(context)))
        {
            sb.Append(effectText);
        }

        return sb.ToString();
    }

    public void AddEffect(IEffect effect)
    {
        var effectDescriptor = effect.EffectExpression.CreateDescriptor();
        
        foreach (var target in effectDescriptor.GetTargets()
                     .DistinctBy(t => t.TargetIndex))
        {
            if (!_targetDescriptors.TryGetValue(target.TargetIndex, out var t))
            {
                _targetDescriptors[target.TargetIndex] = new TargetDescriptor(target.TargetType, target.TargetIndex);
            }
            else if (t?.TargetType != target.TargetType)
            {
                throw new ArgumentException($"Target index ({target.TargetIndex}) already set to a different type ({t?.TargetType} != {target.TargetType})");
            }
        } 
        
        _effects.Add(effect);
        _effectDescriptors.Add(effectDescriptor);
        _effectFunctions.Add(effect.EffectExpression.Compile());
        
        var newStaticRulesText = string.Join(Environment.NewLine, _effectDescriptors.Select(e => e.GetStaticRulesText()));

        Meta = Meta with
        {
            StaticRulesText = newStaticRulesText
        };
    }

    private record TargetDescriptor(Type TargetType, int TargetIndex) : ITargetDescriptor;
}