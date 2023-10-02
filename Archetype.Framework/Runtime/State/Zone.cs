using Archetype.Framework.Proto;

namespace Archetype.Framework.Runtime.State;

public class Node : Atom, IZone
{
    public override IReadOnlyDictionary<string, KeywordInstance> Characteristics { get; } = 
        Declare.Characteristics(
            ("TYPE", "zone"), 
            ("SUBTYPE", "node")
        );
    public IList<ICard> Cards { get; } = new List<ICard>();
}

public class DrawPile : Atom, IZone
{
    public override IReadOnlyDictionary<string, KeywordInstance> Characteristics { get; } = 
        Declare.Characteristics(
            ("TYPE", "zone"), 
            ("SUBTYPE", "draw pile")
            );
    public IList<ICard> Cards { get; } = new List<ICard>();
}

public class Hand : Atom, IZone
{
    public override IReadOnlyDictionary<string, KeywordInstance> Characteristics { get; } = 
        Declare.Characteristics(
            ("TYPE", "zone"), 
            ("SUBTYPE", "hand")
            );
    public IList<ICard> Cards { get; } = new List<ICard>();
}

public class DiscardPile : Atom, IZone
{
    public override IReadOnlyDictionary<string, KeywordInstance> Characteristics { get; } = 
        Declare.Characteristics(
            ("TYPE", "zone"), 
            ("SUBTYPE", "discard pile")
            );
    public IList<ICard> Cards { get; } = new List<ICard>();
}

public class Exile : Atom, IZone
{
    public override IReadOnlyDictionary<string, KeywordInstance> Characteristics { get; } = 
        Declare.Characteristics(
            ("TYPE", "zone"), 
            ("SUBTYPE", "exile")
            );
    public IList<ICard> Cards { get; } = new List<ICard>();
}

public class Stack : Atom, IZone
{
    public override IReadOnlyDictionary<string, KeywordInstance> Characteristics { get; } = 
        Declare.Characteristics(
            ("TYPE", "zone"), 
            ("SUBTYPE", "stack")
            );
    public IList<ICard> Cards { get; } = new List<ICard>();
}

public class Payment : Atom, IZone
{
    public override IReadOnlyDictionary<string, KeywordInstance> Characteristics { get; } = 
        Declare.Characteristics(
            ("TYPE", "zone"), 
            ("SUBTYPE", "payment")
            );
    public IList<ICard> Cards { get; } = new List<ICard>();
}