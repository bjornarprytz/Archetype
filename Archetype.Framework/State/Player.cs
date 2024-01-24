using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;

namespace Archetype.Framework.State;

public interface IPlayer : IAtom
{
    IOrderedZone Deck { get; }
    IZone Hand { get; }
    IZone DiscardPile { get; }
}

public class Player : Atom, IPlayer
{
    public override IReadOnlyDictionary<string, int> Stats { get; } = new Dictionary<string, int>();
    public override IReadOnlyDictionary<string, string> Tags { get; } = new Dictionary<string, string>
    {
        {  "TYPE", "Player" }
    };

    public IOrderedZone Deck { get; }
    public IZone Hand { get; }
    public IZone DiscardPile { get; }
}