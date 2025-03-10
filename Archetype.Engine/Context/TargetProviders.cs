using Aqua.TypeExtensions;
using Archetype.Core.Atoms;
using Archetype.Core.Atoms.Base;
using Archetype.Core.Exceptions;
using Archetype.Core.Extensions;
using Archetype.Core.Play;
using Archetype.View.Infrastructure;
using Archetype.View.Play;

namespace Archetype.Engine.Context;

internal class RangedTargetProvider : TargetProvider
{
    public RangedTargetProvider(IMapNode whence, int range, IEnumerable<ITargetDescriptor> targetDescriptors, IEnumerable<IGameAtom> chosenTargets) : base(targetDescriptors, chosenTargets)
    {
        foreach (var targetUnit in Targets.Values.SelectMany(t => t).OfType<IUnit>())
        {
            if (targetUnit.CurrentZone is IMapNode node
                && node.DistanceTo(whence) is var distance and > -1 
                && distance > range)
            {
                throw new InvalidOperationException(
                    $"Target is too far from whence. Range: {range}, Distance: {distance}");
            }
        }
    }
}

internal class TargetProvider : ITargetProvider
{
    protected readonly Dictionary<Type, List<IGameAtom>> Targets = new();

    public TargetProvider(IEnumerable<ITargetDescriptor> targetDescriptors, IEnumerable<IGameAtom> chosenTargets)
    {
        var required = targetDescriptors.ToList();
        var chosen = chosenTargets.ToList();
        
        if (required.Count != chosen.Count)
        {
            throw new TargetCountMismatchException(required.Count, chosen.Count);
        }
            
        foreach (var (targetData, chosenTarget) in required.Zip(chosen))
        {
            var targetType = targetData.TargetType;
            
            if (!chosenTarget.GetType().Implements(targetType))
            {
                throw new InvalidTargetChosenException();
            }
            
            Targets.GetOrSet(targetType).Add(chosenTarget);
        }
    }

    public T GetTarget<T>() where T : IGameAtom
    {
        return GetTargetInternal<T>(0);
    }

    public T GetTarget<T>(int index) where T : IGameAtom
    {
        return GetTargetInternal<T>(index);
    }

    private T GetTargetInternal<T>(int index)
    {
        var targetsOfTypeT = Targets[typeof(T)];

        if (targetsOfTypeT is null || targetsOfTypeT.IsEmpty() || targetsOfTypeT.Count <= index)
        {
            throw new Exception($"Can't provide target of type {typeof(T)} and index {index}");
        }
        
        return (T) targetsOfTypeT[index];
    }
}