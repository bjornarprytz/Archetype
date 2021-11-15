using Godot;
using System.Linq;
  
public class TargetArrow : Line2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	public void SetPosition(Vector2 from, Vector2 to)
	{
		while (Points.Any())
		{
			RemovePoint(0);
		}
		
		AddPoint(from);
		AddPoint(to);
		Update();
	}

	public override void _Draw()
	{
		base._Draw();

		
	}

	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
