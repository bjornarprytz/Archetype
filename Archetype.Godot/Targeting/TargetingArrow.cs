using Godot;

namespace Archetype.Godot.Targeting
{
	public class TargetingArrow : Line2D, ITargetingArrow
	{
		public void Activate()
		{
			ClearPoints();
			
			AddPoint(Vector2.Zero);
			AddPoint(Vector2.Zero);
		}

		public void Deactivate()
		{
			ClearPoints();
		}

		public void SetAnchor(Vector2 anchorPos)
		{
			SetPointPosition(0, anchorPos);
		}

		public void ChangePosition(Vector2 newPos)
		{
			SetPointPosition(1, newPos);
		}

		public Node GetTarget()
		{
			var spaceState = GetWorld2d().DirectSpaceState;
			var result = spaceState.IntersectRay(Vector2.Zero, Points[1]);

			return null; // TODO: Return something real
		}
	}
}
