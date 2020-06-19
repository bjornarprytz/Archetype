using System;

namespace Archetype
{
    public abstract class GamePiece
    {
        public Guid Id { get; private set; }
        public Faction Team { get; private set; }

        public bool IsAllyOf(GamePiece other) => Team == other.Team;
        public bool IsEnemyOf(GamePiece other) => Team != other.Team;

        public bool IsMe(GamePiece other) => Id == other.Id;
        public bool IsOther(GamePiece other) => Id != other.Id;

        

        public GamePiece(Faction team = Faction.Neutral)
        {
            Team = team;
            Id = Guid.NewGuid();
        }
    }
}