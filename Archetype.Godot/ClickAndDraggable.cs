using Godot;

public class ClickAndDraggable : Area2D
{
	private bool isHeld;

	private TargetArrow dragArrow;

	public ClickAndDraggable()
	{
		// Abstract the OkAction, which is triggered by the following events (Enter and K)
		InputMap.AddAction("OkAction");
		InputMap.ActionAddEvent("OkAction", new InputEventKey{ Scancode = (int)KeyList.Enter } );
		InputMap.ActionAddEvent("OkAction", new InputEventKey{ Scancode = (int)KeyList.K } );

	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var scene = ResourceLoader.Load<PackedScene>("res://target_arrow.tscn");

		dragArrow = scene.Instance<TargetArrow>();

		AddChild(dragArrow);
	}

	private void _on_Area2D_input_event(object viewport, object @event, int shape_idx)
	{
		if (@event is InputEventMouseButton mb)
		{
			isHeld = mb.Pressed;
		}
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);

		if (!@event.IsActionPressed("OkAction") ) return;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (!isHeld)
		{
			return;
		}

		switch (@event)
		{
			case InputEventMouseMotion mm:
				dragArrow.SetPosition(Position, mm.Position);
				break;
			case InputEventMouseButton mb:
				isHeld = mb.Pressed;
				break;
		}
	}


	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}


