using System;
using Archetype.Godot.Extensions;
using Godot;

namespace Archetype.Godot;

public class CameraController : Spatial
{
	private const float Acceleration = 0.008f;
	private const float Deceleration = 0.05f;
	private const float TopSpeed = 0.4f;
	
	private float _velocity = 0f;
	private Vector3 _direction;

	public override void _Process(float delta)
	{
		var dir = InputToDirection();

		if (dir == Vector3.Zero)
		{
			_velocity = Math.Max(0f, _velocity - Deceleration);
		}
		else
		{
			_velocity = Math.Min(TopSpeed, _velocity + Acceleration);

			_direction = dir.Normalized();
		}

		GlobalTranslate(_direction
			.Rotated(Vector3.Up, -(float)Math.PI / 2) 
						* _velocity);
	}

	private static Vector3 InputToDirection()
	{
		var dir = Vector3.Zero;

		if (Input.IsKeyPressed((int)KeyList.W))
		{
			dir += Vector3.Forward;
		}
		if (Input.IsKeyPressed((int)KeyList.S))
		{
			dir += Vector3.Back;
		}

		if (Input.IsKeyPressed((int)KeyList.A))
		{
			dir += Vector3.Left;
		}

		if (Input.IsKeyPressed((int)KeyList.D))
		{
			dir += Vector3.Right;
		}

		return dir;
	}
}
