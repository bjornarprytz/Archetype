using Archetype.Core.Atoms;
using Archetype.Core.Atoms.Cards;
using Archetype.Core.Effects;
using Archetype.Core.Infrastructure;
using Archetype.Rules.Extensions;

namespace Archetype.Rules.Prompts;

public readonly struct DiscardPrompt : IPromptResolver<ICard>
{
    public string PromptType => "Discard";
    public IEnumerable<Guid> EligibleAtoms { get; private init; }
    public int MaxAnswers { get; private init; }
    public int MinAnswers { get; private init; }
    public IResult Resolve(IGameState gameState, IEnumerable<IAtom> atoms)
    {
        return Resolve(gameState, atoms.OfType<ICard>());
    }

    public static IPrompt FromState(IGameState state, int min, int max)
    {
        return new DiscardPrompt{
            EligibleAtoms = state.Player.Hand.Contents.Select(x => x.Id),
            MinAnswers = min,
            MaxAnswers = max,
        };
    }

    public IResult Resolve(IGameState gameState, IEnumerable<ICard> cards)
    {
        return IResult.Join(
            cards.Select(
                    card => card.MoveTo(gameState.Player.DiscardPile))
                .ToList()); // Force evaluation
    }
}
