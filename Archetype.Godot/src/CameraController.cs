using System;
using Archetype.Godot.Extensions;
using Godot;

namespace Archetype.Godot;

public class CameraController : Spatial
{
	private const float Acceleration = 0.01f;
	private const float Deceleration = 0.02f;
	private const float TopSpeed = 0.5f;
	
	private float _velocity = 0f;
	private Vector2 _direction;
	
	public override void _Process(float delta)
	{
		var dir = Vector2.Zero;
		
		if (Input.IsActionPressed("ui_up")) // TODO: Check out why this throws MissingMethodException in the editor
		{
			dir += Vector2.Up;
		}
		if (Input.IsActionPressed("ui_down"))
		{
			dir += Vector2.Down; 
		}
		if (Input.IsActionPressed("ui_left"))
		{
			dir += Vector2.Left;
		}
		if (Input.IsActionPressed("ui_right"))
		{
			dir += Vector2.Right;
		}


		if (dir == Vector2.Zero)
		{
			_velocity = Math.Max(0f, _velocity - Deceleration);
		}
		else
		{
			_velocity = Math.Min(TopSpeed, _velocity + Acceleration); 
			
			_direction = dir.Normalized();
		}
		
		Translate(_direction.ToVector3().Rotated(Vector3.Right, (float)Math.PI / 2f ) * _velocity);
	}
}
