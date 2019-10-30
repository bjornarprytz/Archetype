using System;

namespace Archetype
{
    public class GamePiece
    {
        public Guid Id { get; private set; }

        public GamePiece()
        {
            Id = Guid.NewGuid();
        }
    }
}