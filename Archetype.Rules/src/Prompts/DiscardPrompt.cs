using Archetype.Core.Atoms;
using Archetype.Core.Atoms.Cards;
using Archetype.Core.Infrastructure;
using Archetype.Core.Prompts;

namespace Archetype.Rules.Prompts;

public readonly struct DiscardPrompt : IPromptResolver<ICard>
{
    public string PromptType => "Discard";
    public IEnumerable<Guid> EligibleAtoms { get; private init; }
    public int MaxAnswers { get; private init; }
    public int MinAnswers { get; private init; }
    public void Resolve(IGameState gameState, IEnumerable<IAtom> atoms)
    {
        Resolve(gameState, atoms.OfType<ICard>());
    }

    public static IPrompt FromState(IGameState state, int min, int max)
    {
        return new DiscardPrompt{
            EligibleAtoms = state.Player.Hand.Contents.Select(x => x.Id),
            MinAnswers = min,
            MaxAnswers = max,
        };
    }

    public void Resolve(IGameState gameState, IEnumerable<ICard> cards)
    {
        foreach (var card in cards)
        {
            card.MoveTo(gameState.Player.DiscardPile);
        }
    }
}
