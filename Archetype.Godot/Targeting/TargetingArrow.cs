using System;
using System.Reactive.Subjects;
using Godot;

namespace Archetype.Godot.Targeting
{
	public class TargetingArrow : Line2D, ITargetingArrow
	{
		private readonly Subject<ITargetable> _onTarget = new();

		public IObservable<ITargetable> OnTarget => _onTarget;
		
		public override void _Ready()
		{
			base._Ready();
			
			AddPoint(Vector2.Zero);
			AddPoint(Vector2.Zero);
		}

		public override void _Input(InputEvent @event)
		{
			base._Input(@event);
			
			switch (@event)
			{
				case InputEventMouseButton { Pressed: false }:
					TryTarget();
					break;
				case InputEventMouseMotion mm:
					SetPointPosition(1, mm.Position);
					break;
			}
		}

		public void SetAnchor(Vector2 anchorPos)
		{
			SetPointPosition(0, anchorPos);
		}

		private void TryTarget()
		{
			var spaceState = GetWorld2d().DirectSpaceState;
			var result = spaceState.IntersectPoint(Points[1], collideWithAreas: true);

			if (result == null 
				|| result.Count == 0 
				|| result[0] is not ITargetable targetable) return;

			_onTarget.OnNext(targetable);
		}

		
	}
}
