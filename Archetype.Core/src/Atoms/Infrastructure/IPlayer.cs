using Archetype.Core.Atoms.Zones;

namespace Archetype.Core.Atoms.Infrastructure;

public interface IPlayer
{
    public int Life { get; set; }
    
    public IDeck Deck { get; }
    public IHand Hand { get; }
    public IDiscardPile DiscardPile { get; }
}