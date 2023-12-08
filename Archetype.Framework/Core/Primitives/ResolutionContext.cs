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
    public IReadOnlyDictionary<CostType, PaymentPayload> Payments { get; }
    public IReadOnlyList<IAtom> Targets { get; }
    public IReadOnlyList<int> ComputedValues { get; }

    public IDictionary<Guid, IReadOnlyList<IAtom>> PromptResponses { get; }
    public IList<IEvent> Events { get; }
    public IDictionary<string, object> Memory { get; } // TODO: Evaluate if this is needed
}

public class ResolutionContext : IResolutionContext
{
    public required IMetaGameState MetaGameState { get; init; }
    public required IGameState GameState { get; init; }
    public required IAtom Source { get; init; }

    public IReadOnlyDictionary<CostType, PaymentPayload> Payments { get; init; } 
        = FrozenDictionary<CostType, PaymentPayload>.Empty; 
    public IReadOnlyList<IAtom> Targets { get; init; } 
        = ArraySegment<IAtom>.Empty;
    public IReadOnlyList<int> ComputedValues { get; init; } 
        = ArraySegment<int>.Empty;

    public IDictionary<Guid, IReadOnlyList<IAtom>> PromptResponses { get; } = new Dictionary<Guid, IReadOnlyList<IAtom>>();
    public IList<IEvent> Events { get; } = new List<IEvent>();
    public IDictionary<string, object> Memory { get; } = new Dictionary<string, object>();
}