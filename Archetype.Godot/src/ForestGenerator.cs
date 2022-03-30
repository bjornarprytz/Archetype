using Godot;
using System;
using Archetype.Godot.Extensions;

public class ForestGenerator : Spatial
{
	private readonly PackedScene[] _treeScenes = new PackedScene[4];
	private Vector3 _forestBoundsSize;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_treeScenes[0] = ResourceLoader.Load<PackedScene>("scn/trees/tree01.tscn");
		_treeScenes[1] = ResourceLoader.Load<PackedScene>("scn/trees/tree02.tscn");
		_treeScenes[2] = ResourceLoader.Load<PackedScene>("scn/trees/tree03.tscn");
		_treeScenes[3] = ResourceLoader.Load<PackedScene>("scn/trees/tree04.tscn");

		
		_forestBoundsSize = GetNode<MeshInstance>("Floor").Transform.Size();
	}

	public void GenerateForest(int forestSize=500)
	{
		for(var i=0; i < forestSize; i++)
		{
			if (_treeScenes.PickOneRandom().Instance() is not Spatial tree)
			{
				throw new Exception("Missing tree scene");
			}
			
			this.AddChild(tree);

			var position = _forestBoundsSize.RandomInCenteredBounds();

			tree.Translate(position);
		}
	}
}
