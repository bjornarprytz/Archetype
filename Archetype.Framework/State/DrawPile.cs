using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;

namespace Archetype.Framework.State;

public class DrawPile : Zone, IOrderedZone
{
    private readonly List<ICard> _cards = new();

    public override void Add(IAtom atom)
    {
        base.Add(atom);
        _cards.Add((ICard)atom);
    }
    
    public override void Remove(IAtom atom)
    {
        base.Remove(atom);
        _cards.Remove((ICard)atom);
    }
    
    public override IReadOnlyDictionary<string, int> Stats { get; } = new Dictionary<string, int>();
    public override IReadOnlyDictionary<string, string> Tags { get; } = new Dictionary<string, string>
    {
        {  "TYPE", "DrawPile" }
    };
    
    public void Shuffle()
    {
        _cards.Shuffle();
    }

    public IAtom? PeekTop()
    {
        return _cards.FirstOrDefault();
    }
}