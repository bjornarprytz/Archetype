using Archetype.Framework.Proto;

namespace Archetype.Framework.Runtime.State;

public interface INode : IZone { }
public interface IDrawPile : IZone { ICard GetTopCard(); }
public interface IHand : IZone { }
public interface IDiscardPile : IZone { }
public interface IExile : IZone { }
public interface IStack : IZone { }
public interface IPayment : IZone { }

public class Node : Atom, INode
{
    public override IReadOnlyDictionary<string, IKeywordInstance> Characteristics { get; } = 
        Declare.Characteristics(
            ("TYPE", "zone"), 
            ("SUBTYPE", "node")
        );
    public IList<ICard> Cards { get; } = new List<ICard>();
}

public class DrawPile : Atom, IDrawPile
{
    public override IReadOnlyDictionary<string, IKeywordInstance> Characteristics { get; } = 
        Declare.Characteristics(
            ("TYPE", "zone"), 
            ("SUBTYPE", "draw pile")
            );
    public IList<ICard> Cards { get; } = new List<ICard>();
    public ICard? GetTopCard()
    {
        return Cards.LastOrDefault();
    }
}

public class Hand : Atom, IHand
{
    public override IReadOnlyDictionary<string, IKeywordInstance> Characteristics { get; } = 
        Declare.Characteristics(
            ("TYPE", "zone"), 
            ("SUBTYPE", "hand")
            );
    public IList<ICard> Cards { get; } = new List<ICard>();
}

public class DiscardPile : Atom, IDiscardPile
{
    public override IReadOnlyDictionary<string, IKeywordInstance> Characteristics { get; } = 
        Declare.Characteristics(
            ("TYPE", "zone"), 
            ("SUBTYPE", "discard pile")
            );
    public IList<ICard> Cards { get; } = new List<ICard>();
}

public class Exile : Atom, IExile
{
    public override IReadOnlyDictionary<string, IKeywordInstance> Characteristics { get; } = 
        Declare.Characteristics(
            ("TYPE", "zone"), 
            ("SUBTYPE", "exile")
            );
    public IList<ICard> Cards { get; } = new List<ICard>();
}

public class Stack : Atom, IStack
{
    public override IReadOnlyDictionary<string, IKeywordInstance> Characteristics { get; } = 
        Declare.Characteristics(
            ("TYPE", "zone"), 
            ("SUBTYPE", "stack")
            );
    public IList<ICard> Cards { get; } = new List<ICard>();
}

public class Payment : Atom, IPayment
{
    public override IReadOnlyDictionary<string, IKeywordInstance> Characteristics { get; } = 
        Declare.Characteristics(
            ("TYPE", "zone"), 
            ("SUBTYPE", "payment")
            );
    public IList<ICard> Cards { get; } = new List<ICard>();
}