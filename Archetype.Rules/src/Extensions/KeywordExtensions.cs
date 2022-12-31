using Archetype.Core.Atoms;
using Archetype.Core.Atoms.Zones;
using Archetype.Core.Effects;
using Archetype.Core.Extensions;
using Archetype.Core.Meta;

namespace Archetype.Rules.Extensions;

public static class KeywordExtensions
{
    [Keyword("Move <TAtom> to {0}")]
    public static IResult MoveTo<TAtom>(this TAtom atom, IZone<TAtom> destination)
        where TAtom : IAtom, IZoned<TAtom>
    {
        var source = atom.CurrentZone;

        atom.CurrentZone?.Remove(atom);
        destination.Add(atom);
        atom.CurrentZone = destination;

        return
            IResult.From(
                new EffectResult(
                    "Move",
                    new Dictionary<string, string>()
                    {
                        { "Atom", atom.Id.ToString() },
                        { "Source", source?.Id.ToString() },
                        { "Destination", destination.Id.ToString() }
                    })
            );
    }
    
    public static IResult Damage<TAtom>(this TAtom atom, int amount)
        where TAtom : IAtom, IHealth
    {
        atom.CurrentHealth -= amount;

        return
            IResult.From(
                new EffectResult(
                    "Damage",
                    new Dictionary<string, string>()
                    {
                        { "Atom", atom.Id.ToString() },
                        { "Amount", amount.ToString() }
                    })
            );
    }

    public static IResult DrawCard(this IPlayer player)
    {
        var card = player.DrawPile.PeekTopCard();

        return card.MoveTo(player.Hand);
    }

    public static IResult ShuffleDrawPile(this IPlayer player)
    {
        player.DrawPile.Shuffle();

        return IResult.From(
            new EffectResult(
                "Shuffle",
                new Dictionary<string, string>()
                {
                    { "Atom", player.DrawPile.Id.ToString() },
                })
        );
    }

    private record EffectResult(string Keyword, IReadOnlyDictionary<string, string> Data) : IEffectResult;
}