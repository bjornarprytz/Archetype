using Godot;
using System;

public class Healthbar : Sprite3D
{
	private TextureProgress _progressBar;
	
	public int Value { get; private set; }
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_progressBar = GetNode<TextureProgress>("Viewport/TextureProgress");
	}

	public void SetHealth(int value)
	{
		Value = value;
		
		_progressBar.Value = value;
		
	}
}
