using System;

namespace Archetype
{
    public abstract class GamePiece
    {
        public Guid Id { get; private set; }
        public Faction Team { get; private set; }

        public bool AllyOf(GamePiece other) => Team == other.Team;

        public GamePiece(Faction team = Faction.Neutral)
        {
            Team = team;
            Id = Guid.NewGuid();
        }
    }
}