
namespace Archetype.Game.Payloads.Pieces
{
    public interface IPlayer : IGamePiece
    {
        IHand Hand { get; }
        IDiscardPile DiscardPile { get; }
        int Resources { get; set; }
    }
}
