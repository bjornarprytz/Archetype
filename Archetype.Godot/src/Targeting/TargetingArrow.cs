using System.Collections.Generic;
using Godot;
using Godot.Collections;

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

		public void Reset()
		{
			SetPointPosition(1, Vector2.Zero);
		}
		
		public bool TryTarget(out ITargetable targetable)
		{
			targetable = null;
			
			var spaceState = GetWorld2d().DirectSpaceState;
			var result = spaceState.IntersectPoint(GetPointPosition(1), collideWithAreas: true);

			if (result == null 
				|| result.Count == 0 
				|| result[0] is not Dictionary d 
				|| d["collider"] is not ITargetable t) return false;

			targetable = t;
			return true;
		}
	}
}
