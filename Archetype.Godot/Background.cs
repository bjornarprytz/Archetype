using Godot;
using System;

public class Background : Control
{
	private readonly Vector2 _minimumSize = new (800, 600);

	private Viewport _viewport;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_viewport = GetViewport();
		
		_viewport.Connect(Signals.Viewport.SizeChanged, this, nameof(WindowResize));
	}

	private void WindowResize()
	{
		var currentSize = OS.WindowSize;

		var scaleFactor = _minimumSize.y / currentSize.y;

		var newSize = new Vector2(currentSize.x * scaleFactor, _minimumSize.y);

		if (newSize.y < _minimumSize.y)
		{
			scaleFactor = _minimumSize.y / newSize.y;
			newSize = new Vector2(newSize.x * scaleFactor, _minimumSize.y);
		}

		if (newSize.x < _minimumSize.x)
		{
			scaleFactor = _minimumSize.x / newSize.x;
			newSize = new Vector2(_minimumSize.x, newSize.y * scaleFactor);
		}
		
		_viewport.SetSizeOverride(true, newSize);
	}
}
