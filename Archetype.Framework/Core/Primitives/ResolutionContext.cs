using System.Collections.Frozen;
using Archetype.Framework.Core.Structure;
using Archetype.Framework.Interface.Actions;
using Archetype.Framework.State;

namespace Archetype.Framework.Core.Primitives;

public interface IResolutionContext
{
    public IMetaGameState MetaGameState { get; }
    public IGameState GameState { get; }
    public IAtom Source { get; }
    public IReadOnlyDictionary<CostType, IReadOnlyList<IAtom>> Payments { get; }
    public IReadOnlyList<IAtom> Targets { get; }
    public IReadOnlyList<int> ComputedValues { get; }

    public IDictionary<Guid, IReadOnlyList<IAtom>> PromptResponses { get; }
    public IList<IEvent> Events { get; }
}

public record ResolutionContext(IMetaGameState MetaGameState, IGameState GameState, IAtom Source) : IResolutionContext
{
    public IReadOnlyDictionary<CostType, IReadOnlyList<IAtom>> Payments { get; init; } 
        = FrozenDictionary<CostType, IReadOnlyList<IAtom>>.Empty; 
    public IReadOnlyList<IAtom> Targets { get; init; } 
        = ArraySegment<IAtom>.Empty;
    public IReadOnlyList<int> ComputedValues { get; init; } 
        = ArraySegment<int>.Empty;

    public IDictionary<Guid, IReadOnlyList<IAtom>> PromptResponses { get; } = new Dictionary<Guid, IReadOnlyList<IAtom>>();
    public IList<IEvent> Events { get; } = new List<IEvent>();
}