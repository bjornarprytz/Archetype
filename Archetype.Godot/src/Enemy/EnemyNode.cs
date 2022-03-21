using System;
using Archetype.Prototype1Data;
using Godot;

namespace Archetype.Godot.Enemy;

public class EnemyNode : Spatial
{
	private readonly Random _random = new (69); // TODO: Remove
	
	private readonly EnemyStateMachine _stateMachine;
	private Healthbar _healthbar;
	public EnemyNode()
	{
		_stateMachine = new EnemyStateMachine(this);
	}
	
	public void Load(IEnemy enemyData)
	{
		// TODO: Load Data
	}

	public override void _Ready()
	{
		_healthbar = GetNode<Healthbar>("Healthbar");
	}

	public override void _Process(float delta)
	{
		var health = (_healthbar.Value + 1) % 100;
		
		_healthbar.SetHealth(health);
		
		GD.Print(_healthbar.Value);
	}
}
