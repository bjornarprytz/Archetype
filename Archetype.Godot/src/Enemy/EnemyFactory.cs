using Archetype.Godot.Infrastructure;
using Archetype.Prototype1Data;

namespace Archetype.Godot.Enemy;

public interface IEnemyFactory
{
    EnemyNode Create(IEnemy enemyData);
}

public class EnemyFactory : IEnemyFactory
{
    private readonly ISceneFactory _sceneFactory;

    public EnemyFactory(ISceneFactory sceneFactory)
    {
        _sceneFactory = sceneFactory;
    }
    
    public EnemyNode Create(IEnemy enemyData)
    {
        var enemyNode = _sceneFactory.CreateNode<EnemyNode>();

        enemyNode.Load(enemyData);

        return enemyNode;
    }
}