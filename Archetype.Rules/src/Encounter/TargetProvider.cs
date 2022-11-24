using Aqua.TypeExtensions;
using Archetype.Core.Atoms;
using Archetype.Core.Effects;
using Archetype.Core.Extensions;

namespace Archetype.Rules.Encounter;

internal class TargetProvider : ITargetProvider
{
    private readonly Dictionary<Type, List<IAtom>> _targets = new();

    public TargetProvider(IEnumerable<IAtom> chosenTargets, IEnumerable<ITargetDescriptor> targetDescriptors)
    {
        var required = targetDescriptors.ToList();
        var chosen = chosenTargets.ToList();
    
        if (required.Count != chosen.Count)
        {
            throw new ArgumentException($"Number of targets ({chosen.Count}) does not match required number of targets ({required.Count})");
        }
        
        foreach (var (targetData, chosenTarget) in required.Zip(chosen))
        {
            var targetType = targetData.TargetType;
        
            if (!chosenTarget.GetType().Implements(targetType))
            {
                throw new ArgumentException("Target does not match required target type");
            }
        
            _targets.GetOrSet(targetType).Add(chosenTarget);
        }
    }
    public T GetTarget<T>() where T : IAtom
    {
        return GetTargetInternal<T>(0);
    }

    public T GetTarget<T>(int index) where T : IAtom
    {
        return GetTargetInternal<T>(index);
    }

    private T GetTargetInternal<T>(int index)
    {
        var targetsOfTypeT = _targets[typeof(T)];

        if (targetsOfTypeT.IsEmpty() || targetsOfTypeT.Count <= index)
        {
            throw new Exception($"Can't provide target of type {typeof(T)} and index {index}");
        }
    
        return (T) targetsOfTypeT[index];
    }
}