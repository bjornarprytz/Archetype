using System;
using System.Reactive.Subjects;
using Godot;

namespace Archetype.Godot.Targeting
{
	public class TargetingArrow : Line2D
	{
		public override void _Ready()
		{
			base._Ready();
			
			AddPoint(Vector2.Zero);
			AddPoint(Vector2.Zero);
		}

		public void SetAnchor(Vector2 anchorPos)
		{
			SetPointPosition(0, anchorPos);
		}

		public void PointTo(Vector2 target)
		{
			SetPointPosition(1, target);
		}
	}
}
