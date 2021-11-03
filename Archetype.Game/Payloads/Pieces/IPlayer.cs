
namespace Archetype.Game.Payloads.Pieces
{
    public interface IPlayer : IGamePiece
    {
        IDeck Deck { get; }
        IHand Hand { get; }
        IDiscardPile DiscardPile { get; }
        int Resources { get; set; }
    }
}
