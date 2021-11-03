using System;

namespace Archetype.Core.Data.Instance
{
    public abstract class GamePieceInstance
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
    }
}