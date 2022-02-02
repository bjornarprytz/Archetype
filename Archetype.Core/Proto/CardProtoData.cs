using Archetype.Core.Extensions;
using Archetype.Core.Play;
using Archetype.View.Atoms.MetaData;
using Archetype.View.Extensions;
using Archetype.View.Infrastructure;
using Archetype.View.Infrastructure.Play;
using Archetype.View.Proto;

namespace Archetype.Core.Proto;

public interface ICardProtoData : ICardProtoDataFront
{
    IEnumerable<IEffect> Effects { get; }
}

public class CardProtoData : ProtoData, ICardProtoData
{
    private readonly List<IEffectDescriptor> _effectDescriptors = new ();
    private readonly List<ITargetDescriptor> _targetDescriptors = new (); 
        
    private readonly List<IEffect> _effects;

    public CardProtoData(List<IEffect> effects)
    {
        _effects = effects;
    }
        
    public int Cost { get; set; }
    public int Range { get; set; }
    public CardMetaData MetaData { get; set; }
    public IEnumerable<ITargetDescriptor> TargetDescriptors => _targetDescriptors;
    public IEnumerable<IEffectDescriptor> EffectDescriptors => _effectDescriptors;
    public IEnumerable<IEffect> Effects => _effects;

    public void GenerateDescriptors()
    {
        _targetDescriptors.Clear();
        _effectDescriptors.Clear();
            
        _effectDescriptors.AddRange(_effects.Select(effect => effect.CreateDescription()));

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

    private record TargetDescriptor(Type TargetType) : ITargetDescriptor
    {
        public string TypeId => TargetType.ReadableFullName();
    }
}