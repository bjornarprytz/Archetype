using Archetype.Prototype1Data;
using Godot;

namespace Archetype.Godot.Enemy;

public class EnemyNode : Spatial
{
	private readonly EnemyStateMachine _stateMachine;
	
	public EnemyNode()
	{
		_stateMachine = new EnemyStateMachine(this);
	}
	
	public void Load(IEnemy enemyData)
	{
		// TODO: Load Data
	}
}
