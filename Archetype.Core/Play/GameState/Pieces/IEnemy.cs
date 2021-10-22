using Archetype.Core.Enemy;

namespace Archetype.Core.Pieces
{
    public interface IEnemy
    {
        EnemyData Data { get; set; }
        
        IDeck Deck { get; }
    }
}