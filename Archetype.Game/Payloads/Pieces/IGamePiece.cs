namespace Archetype.Game.Payloads.Pieces
{
    public interface IGamePiece
    {
        long OwnerId { get; }
        long Id { get; }
    }
}