using Godot;
using System;

public class Background : Control
{
	private readonly Vector2 _minimumSize = new (800, 600);

	private Viewport _viewport;
	
	public override void _Ready()
	{
		_viewport = GetViewport();
		
		_viewport.Connect(Signals.Viewport.SizeChanged, this, nameof(OnWindowResize));
	}

	private void OnWindowResize()
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
