using Archetype.BasicRules;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.State;

namespace Archetype.Tests.Inscryption.Cards;

public interface INode : IZone { }

public interface IDrawPile : IOrderedZone { }
public interface IHand : IZone { }
public interface IDiscardPile : IZone { }
public interface IExile : IZone { }
public interface IStack : IZone { }
public interface IPayment : IZone { }

public abstract class Zone : Atom, IZone
{
    protected readonly List<IAtom> InternalAtoms = new();
    public IReadOnlyList<IAtom> Atoms => InternalAtoms;
    public virtual void Add(IAtom atom)
    {
        if (InternalAtoms.Contains(atom))
            throw new InvalidOperationException("Atom already exists in zone.");
        
        InternalAtoms.Add(atom);
    }

    public virtual void Remove(IAtom atom)
    {
        if (!InternalAtoms.Contains(atom))
            throw new InvalidOperationException("Atom does not exist in zone.");
        
        InternalAtoms.Remove(atom);
    }
}

public class Node : Zone, INode
{
    public override IReadOnlyDictionary<string, IKeywordInstance> Characteristics { get; } = 
        Declare.Characteristics(
            ("TYPE", "zone"), 
            ("SUBTYPE", "node")
        );
}

public class DrawPile : Zone, IDrawPile
{
    private readonly List<ICard> _order = new();
    
    public override IReadOnlyDictionary<string, IKeywordInstance> Characteristics { get; } = 
        Declare.Characteristics(
            ("TYPE", "zone"), 
            ("SUBTYPE", "draw pile")
            );

    public void Shuffle()
    {
        _order.Shuffle();
    }

    public override void Add(IAtom atom)
    {
        base.Add(atom);
        _order.Add((ICard) atom);
    }
    
    public override void Remove(IAtom atom)
    {
        _order.Remove((ICard) atom);
        base.Remove(atom);
    }

    public IAtom? PeekTop()
    {
        return _order.FirstOrDefault();
    }
}

public class Hand : Zone, IHand
{
    public override IReadOnlyDictionary<string, IKeywordInstance> Characteristics { get; } = 
        Declare.Characteristics(
            ("TYPE", "zone"), 
            ("SUBTYPE", "hand")
            );
}

public class DiscardPile : Zone, IDiscardPile
{
    public override IReadOnlyDictionary<string, IKeywordInstance> Characteristics { get; } = 
        Declare.Characteristics(
            ("TYPE", "zone"), 
            ("SUBTYPE", "discard pile")
            );
}

public class Exile : Zone, IExile
{
    public override IReadOnlyDictionary<string, IKeywordInstance> Characteristics { get; } = 
        Declare.Characteristics(
            ("TYPE", "zone"), 
            ("SUBTYPE", "exile")
            );
}

public class Stack : Zone, IStack
{
    public override IReadOnlyDictionary<string, IKeywordInstance> Characteristics { get; } = 
        Declare.Characteristics(
            ("TYPE", "zone"), 
            ("SUBTYPE", "stack")
            );
}

public class Payment : Zone, IPayment
{
    public override IReadOnlyDictionary<string, IKeywordInstance> Characteristics { get; } = 
        Declare.Characteristics(
            ("TYPE", "zone"), 
            ("SUBTYPE", "payment")
            );
}