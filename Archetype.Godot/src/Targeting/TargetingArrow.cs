using System;
using System.Collections.Generic;
using Archetype.Godot.Extensions;
using Godot;

namespace Archetype.Godot.Targeting
{
	public class TargetingArrow : ImmediateGeometry
	{
		private Vector2 _mousePosition;
		private MathyExtensions.BezierParameters _bezierParameters;


		public override void _Input(InputEvent @event)
		{
			if (@event is not InputEventMouseMotion mm)
				return;

			_mousePosition = mm.Position;
		}

		public override void _PhysicsProcess(float delta)
		{
			var result = this.CastRayFromMousePosition(_mousePosition, collideWithAreas:true);

			_bezierParameters = result.Hit 
				? new MathyExtensions.BezierParameters(Vector3.Zero, Transform.Up() * 3, result.Normal * 3, ToLocal(result.Position)) 
				: default;
		}
		
		public override void _Process(float delta)
		{
			base._Process(delta);
			
			Clear();
			Begin(Mesh.PrimitiveType.LineStrip);

			foreach (var point in _bezierParameters.BezierPoints(10))
			{
				AddVertex(point);
			}
			
			End();
		}

		

		
		
		
		
	}
}
