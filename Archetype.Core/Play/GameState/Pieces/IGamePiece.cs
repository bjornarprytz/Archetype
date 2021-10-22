namespace Archetype.Core
{
    public interface IGamePiece
    {
        long OwnerId { get; }
        long Id { get; }
    }
}