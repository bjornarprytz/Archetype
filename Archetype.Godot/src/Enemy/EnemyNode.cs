using System;
using Archetype.Godot.Infrastructure;
using Archetype.Godot.StateMachine;
using Archetype.Prototype1Data;
using Godot;
using Stateless;

namespace Archetype.Godot.Enemy;

public class EnemyNode : Spatial
{
	private readonly Random _random = new (69); // TODO: Remove
	
	private Healthbar _healthbar;
	private StateMachine<IState<EnemyNode>, State.Trigger> _stateMachine;

	[Inject]
	public void Construct(
		State.Idle idle,
		State.Moving moving
	)
	{
		_stateMachine = new StateMachine<IState<EnemyNode>, State.Trigger>(idle);
		
		_stateMachine.OnTransitioned(state =>
		{
			state.Source.OnExit(this);
			state.Destination.OnEnter(this);
		});
		
		_stateMachine.Configure(idle)
			.Permit(State.Trigger.StartMoving, moving);

		_stateMachine.Configure(moving)
			.Permit(State.Trigger.StopMoving, idle);
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
	
	public sealed class State
	{
		public enum Trigger
		{
			StartMoving,
			StopMoving
		}

		
		public class Idle : State<EnemyNode>
		{ }
		
		public class Moving : State<EnemyNode>
		{
			public override void OnEnter(EnemyNode model)
			{
				GD.Print($"{model.Name} started moving TODO: Do something here?");
			}

			public override void OnExit(EnemyNode model)
			{
				GD.Print($"{model.Name} stopped moving TODO: Do something here?");
			}
		}
	}
}
