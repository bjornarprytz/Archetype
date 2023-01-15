using Archetype.Core.Atoms;
using Archetype.Core.Atoms.Zones;
using Archetype.Core.Effects;
using Archetype.Core.Meta;

namespace Archetype.Rules.Extensions;

public static class KeywordExtensions
{
    [Keyword("Move to {0}")]
    public static IResult MoveTo<TAtom>(this TAtom atom, IZone<TAtom> destination)
        where TAtom : IAtom, IZoned
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
                        { "Source", source?.Id.ToString() ?? "None" },
                        { "Destination", destination.Id.ToString() }
                    })
            );
    }
    
    [Keyword("Deal {0} damage")]
    public static IResult Damage<TAtom>(this TAtom atom, int amount)
        where TAtom : IAtom, IHealth
    {
        amount = int.Clamp(amount, 0, atom.CurrentHealth);
        
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
    
    [Keyword("Attack")]
    public static IResult Attack<TAttacker, TTarget>(this TAttacker attacker, TTarget target)
        where TAttacker : IAtom, IPower
        where TTarget : IAtom, IHealth
    {
        var damageResult = target.Damage(attacker.Power);

        return
            IResult.Join(
                damageResult,
                IResult.From(
                    new EffectResult(
                        "Attack",
                        new Dictionary<string, string>()
                        {
                            { "Attacker", attacker.Id.ToString() },
                            { "Target", target.Id.ToString() }
                        })
                )
            );
    }

    [Keyword("Draw a card")]
    public static IResult DrawCard(this IPlayer player)
    {
        return player.DrawPile.PeekTopCard() is not { } card 
            ? IResult.Empty()
            : card.MoveTo(player.Hand);
    }

    [Keyword("Shuffle Draw Pile")]
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