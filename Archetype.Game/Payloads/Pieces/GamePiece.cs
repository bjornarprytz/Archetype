using System;

namespace Archetype.Game.Payloads.Pieces
{
    public interface IGamePiece
    {
        Guid Id { get; }
        IGamePiece Owner { get; }
    }

    public abstract class GamePiece : IGamePiece
    {
        protected GamePiece(IGamePiece owner)
        {
            Id = Guid.NewGuid();
            Owner = owner ??= this;
        }

        public Guid Id { get; }
        public IGamePiece Owner { get; }
    }
}