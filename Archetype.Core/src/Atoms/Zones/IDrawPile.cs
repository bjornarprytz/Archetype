using Archetype.Core.Atoms.Cards;

namespace Archetype.Core.Atoms.Zones;

public interface IDrawPile : IZone<ICard>
{
    public int Count { get; }

    public ICard? PeekTopCard();
    public void Shuffle();
}