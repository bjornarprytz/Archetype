using System.Collections.Generic;
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

		public void PointTo(Vector3 target)
		{
			Curve.SetPointPosition(1, target);
		}

		public void Reset()
		{
			Curve.ClearPoints();
		}
	}
}
