using Archetype.Components.Extensions;
using Archetype.Components.Meta;
using Archetype.Core.Atoms;
using Archetype.Core.Effects;
using Archetype.Core.Proto.PlayingCard;

namespace Archetype.Components.Protos;

internal class ProtoSpell : ProtoCard, IProtoSpell
{
    private readonly List<IEffect> _effects = new ();
    private readonly Dictionary<int, ITargetDescriptor> _targetDescriptors = new ();
        
    private readonly List<Func<IContext, IResult>> _effectFunctions = new ();

    public override IEnumerable<ITargetDescriptor> TargetDescriptors => _targetDescriptors.OrderBy(t => t.Key).Select(t => t.Value);
    public override IResult Resolve(IContext<ICard> context)
    {
        return Result.Aggregate(_effectFunctions.Select(f => f(context)));
    }

    public override string ContextualRulesText(IContext<ICard> context)
    {
        // TODO: Figure out how to structure RulesText so that it's easy for the client to identify variables, and keywords
        
        throw new NotImplementedException();
    }

    public void AddEffect(IEffect effect)
    {
        var effectDescriptor = effect.ResolveExpression.CreateDescriptor();
        
        // TODO: Update static rules text here as well
        
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
        _effectFunctions.Add(effect.ResolveExpression.Compile());
    }

    private record TargetDescriptor(Type TargetType, int TargetIndex) : ITargetDescriptor;
}