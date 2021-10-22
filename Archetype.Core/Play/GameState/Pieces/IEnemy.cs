using Archetype.Core.Enemy;

namespace Archetype.Core
{
    public interface IEnemy : IGamePiece
    {
        EnemyData Data { get; set; }
        
        IDeck Deck { get; }
    }
}