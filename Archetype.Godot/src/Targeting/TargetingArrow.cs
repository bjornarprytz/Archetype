using Archetype.Godot.Extensions;
using Archetype.Godot.StateMachine;
using Godot;
using Godot.Collections;

namespace Archetype.Godot.Targeting
{
	public class TargetingArrow : Path
	{
		private ImmediateGeometry _lineDrawer;
		private Vector2 _mousePosition;
		
		public override void _Ready()
		{
			base._Ready();
			Curve.AddPoint(Vector3.Zero);
			Curve.AddPoint(Vector3.Zero);
			
			_lineDrawer = GetNode<ImmediateGeometry>("LineDrawer");
		}

		public override void _Input(InputEvent @event)
		{
			if (@event is not InputEventMouseMotion mm)
				return;

			_mousePosition = mm.Position;
		}

		public override void _PhysicsProcess(float delta)
		{
			var result = this.CastRayFromMousePosition(_mousePosition, collideWithAreas:true);

			if (result.Hit)
			{
				PointTo(result.Position);
			}
		}

		private void PointTo(Vector3 worldPosition)
		{
			Curve.SetPointPosition(1, ToLocal(worldPosition));
		}
		
		
		public override void _Process(float delta)
		{
			base._Process(delta);
			
			_lineDrawer.Clear();
			_lineDrawer.Begin(Mesh.PrimitiveType.LineStrip);
			
			foreach (var point in Curve.Tessellate())
			{
				_lineDrawer.AddVertex(point);
			}
			
			_lineDrawer.End();
		}
	}
}
