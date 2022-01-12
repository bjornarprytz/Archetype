using System;
using System.Collections.Generic;
using System.Linq;
using Aqua.TypeExtensions;
using Archetype.Game.Exceptions;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Atoms;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.View.Infrastructure;

namespace Archetype.Game.Payloads.Context;

public interface ITargetProvider
{
    T GetTarget<T>() where T : IGameAtom; // TODO: Allow for multiple targets of same type
}

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
            
            (Targets[targetType] ??= new List<IGameAtom>()).Add(chosenTarget);
        }
    }

    public T GetTarget<T>() where T : IGameAtom
    {
        var target = (T)Targets[typeof(T)]?.FirstOrDefault(); // TODO: Allow for multiple targets of same type by indexing them

        if (target is null)
        {
            throw new Exception($"Can't provide target of type {typeof(T)}");
        }
        
        return target;
    }
}