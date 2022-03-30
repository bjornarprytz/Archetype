using System;
using System.Collections.Generic;
using Archetype.Godot.Infrastructure;
using Godot;
using Godot.Collections;

namespace Archetype.Godot.Targeting
{
	public class TargetingArrow : Path
	{
		public override void _Ready()
		{
			base._Ready();
			Curve.AddPoint(Vector3.Zero);
			Curve.AddPoint(Vector3.Zero);
		}

		public override void _Input(InputEvent @event)
		{
			if (@event is not InputEventMouseMotion mm)
				return;

			var point = GetViewport().GetCamera().ProjectRayNormal(mm.Position);
			
			PointTo(point);
		}

		public void PointTo(Vector3 target)
		{
			Curve.SetPointPosition(1, target);
		}
/*
 *TODO: Draw a bezier curve
		public override void _Process(float delta)
		{
			base._Process(delta);
			
			foreach (var point in Curve.Tessellate())
			{
				// TODO: Draw a line
				
				throw new NotImplementedException();
			}

			
		}
 */

		public void Reset()
		{
			Curve.ClearPoints();
		}
		
		
	}
}
