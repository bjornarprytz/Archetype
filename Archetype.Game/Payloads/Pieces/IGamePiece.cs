namespace Archetype.Game.Payloads.Pieces
{
    public interface IGamePiece
    {
        long Id { get; }
        IGamePiece Owner { get; }
    }
}