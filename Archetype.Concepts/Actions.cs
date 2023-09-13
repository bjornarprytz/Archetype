using Archetype.Rules.State;
using MediatR;

namespace Archetype.Rules;

public record PlayCardArgs(Guid Card, IReadOnlyList<Guid> Targets, IReadOnlyList<CostPayload> Payments) : IRequest<Unit>;
public record StartGameArgs(IEnumerable<string> DeckOfCards) : IRequest<Unit>;
public record UseAbilityArgs(Guid AbilitySource, int AbilityIndex, IReadOnlyList<Guid> Targets, IReadOnlyList<CostPayload> Payments) : IRequest<Unit>;
public record PassTurnArgs() : IRequest<Unit>;