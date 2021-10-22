using Archetype.Core.Enemy;

namespace Archetype.Core
{
    public interface IEnemy : IGamePiece
    {
        EnemyData Data { get; set; }
        
        IDeck Deck { get; }
        
        public int Attack(int strength)
        {
            Data.Health -= strength;

            return strength;
        }

        public int Heal(int strength)
        {
            Data.Health += strength;

            return strength;
        }
    }
}