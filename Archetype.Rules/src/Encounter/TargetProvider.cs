using Aqua.TypeExtensions;
using Archetype.Core.Atoms;
using Archetype.Core.Effects;

namespace Archetype.Rules.Encounter;

internal class TargetProvider : ITargetProvider
{
    private readonly List<IAtom> _targets = new();

    public TargetProvider(IEnumerable<IAtom> chosenTargets, IEnumerable<ITargetDescriptor> targetDescriptors)
    {
        var required = targetDescriptors.ToList();
        var chosen = chosenTargets.ToList();
    
        if (required.Count != chosen.Count)
        {
            throw new ArgumentException($"Number of targets ({chosen.Count}) does not match required number of targets ({required.Count})");
        }
        
        foreach (var (requiredTarget, chosenTarget) in required.Zip(chosen))
        {
            var targetType = requiredTarget.TargetType;
        
            if (!chosenTarget.GetType().Implements(targetType))
            {
                throw new ArgumentException("Target does not match required target type");
            }
        
            _targets.Add(chosenTarget);
        }
    }

    public T GetTarget<T>(int index) where T : IAtom
    {
        if (index >= _targets.Count)
        {
            throw new ArgumentException($"Target index ({index}) out of range ({_targets.Count})");
        }
        
        var target = _targets[index];

        if (target is not T typedTarget)
        {
            throw new ArgumentException($"Target at index {index} is not of type {typeof(T).Name}");
        }

        return typedTarget;
    }
}