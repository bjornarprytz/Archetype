using System;

namespace Archetype.Dto.Instance
{
    public abstract class GamePieceInstance
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
    }
}