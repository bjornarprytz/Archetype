using Archetype.Rules.State;
using MediatR;

namespace Archetype.Rules;

public record PlayCardArgs(Card Card, IReadOnlyList<Card> Targets, IReadOnlyList<CostPayload> Payments) : IRequest<Unit>;
public record StartGameArgs(IEnumerable<string> DeckOfCards) : IRequest<Unit>;
public record UseAbilityArgs(Ability Ability, IReadOnlyList<Card> Targets, IReadOnlyList<CostPayload> Payments) : IRequest<Unit>;
public record PassTurnArgs() : IRequest<Unit>;