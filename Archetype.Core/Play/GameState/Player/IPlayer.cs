
namespace Archetype.Core
{
    public interface IPlayer
    {
        IHand Hand { get; }
        IDiscardPile DiscardPile { get; }
        int Resources { get; set; }
    }
}
