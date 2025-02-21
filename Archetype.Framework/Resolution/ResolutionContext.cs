using Archetype.Framework.Core;
using Archetype.Framework.Events;
using Archetype.Framework.GameLoop;
using Archetype.Framework.State;

namespace Archetype.Framework.Resolution;

public interface IResolutionContext : IValueWhence
{
    [PathPart("state")]
    IGameState GetState();
    
    [PathPart("scope")]
    IScope GetScope();
    
    [PathPart("source")]
    IAtom GetSource();
    
    [PathPart("targets")]
    IAtom? GetTarget(int index);
    
    internal IReadOnlyList<EffectProto> Effects { get; }
    internal IReadOnlyList<TargetProto> TargetDescriptors { get; }
    internal IReadOnlyList<IAtom> ChosenTargets { get; }
    internal IEnumerable<IEvent> ResolveEffects();
    internal void BindResolvers(IRules rules);
}

internal class ResolutionContext(IGameState state, IScope scope, ICard source, IEnumerable<IAtom> targets) : IResolutionContext
{
    private bool _contextResolved = false;
    private IEnumerable<Func<IResolutionContext, IEvent>>? _resolvers;
    public IGameState GetState()
    {
        return state;
    }

    public IScope GetScope()
    {
        return scope;
    }

    public IAtom GetSource()
    {
        return source;
    }

    public IAtom? GetTarget(int index)
    {
        return index < ChosenTargets.Count ? ChosenTargets[index] : null;
    }

    public IReadOnlyList<EffectProto> Effects { get; } = source.GetProto().Effects.ToArray();
    public IReadOnlyList<TargetProto> TargetDescriptors { get; } = source.GetProto().Targets.ToArray();
    public IReadOnlyList<IAtom> ChosenTargets { get; } = targets.ToArray();

    public IEnumerable<IEvent> ResolveEffects()
    {
        if (_resolvers == null)
            throw new InvalidOperationException("Resolvers have not been bound");
        
        if (_contextResolved)
            throw new InvalidOperationException("Context has already been resolved");

        var events = new List<IEvent>();
        
        foreach (var @event in _resolvers.Select(effect => effect.Invoke(this)))
        {
            scope.AddEvent(@event);
            events.Add(@event);
        }
        
        _contextResolved = true;

        return events;
    }

    public void BindResolvers(IRules rules)
    {
        if (_resolvers != null)
            throw new InvalidOperationException("Resolvers have already been bound");
        
        if (_contextResolved)
            throw new InvalidOperationException("Context has already been resolved");
        
        var resolvers = Effects.Select(rules.BindEffectResolver).ToList();

        _resolvers = resolvers;
    }
}