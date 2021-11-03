using Archetype.Core.Enemy;

namespace Archetype.Game.Payloads.Pieces
{
    public interface IUnit : IGamePiece
    {
        UnitProtoData ProtoData { get; set; }
        
        IDeck Deck { get; }
        
        public int Attack(int strength)
        {
            ProtoData.Health -= strength;

            return strength;
        }

        public int Heal(int strength)
        {
            ProtoData.Health += strength;

            return strength;
        }
    }
}