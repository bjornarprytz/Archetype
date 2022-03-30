using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Archetype.Godot.Clearing;
using Archetype.Godot.Extensions;
using Archetype.Godot.Infrastructure;
using Godot;
using Archetype.Prototype1Data;

public class MapController : Spatial
{
	
	private IClearingFactory _clearingFactory;
	private Node _clearingContainer;
	private ForestGenerator _forestGenerator;

	[Inject]
	public void Construct(IClearingFactory clearingFactory)
	{
		_clearingFactory = clearingFactory;
	}
	
	public void Load(IMap map)
	{
		base._Ready();
		AddNodeTree(map.Root);

		_forestGenerator.GenerateForest();
	}
	
	public override void _Ready()
	{
		_clearingContainer = GetNode<Node>("Clearings");
		_forestGenerator = GetNode<ForestGenerator>("Forest");
	}

	
	
	private void AddNodeTree(IMapNode node)
	{
		if (node.Neighbours.Count() > 2) 
			throw new NotImplementedException("This function assumes at most two neighbours at the moment");
		
		AddSubtree(node);
	}
	
	private void AddSubtree(IMapNode node, ICollection<IMapNode> handledNodes = null)
	{
		handledNodes ??= new List<IMapNode>();
		
		if (handledNodes.Contains(node))
		{
			return;
		}

		
		
		var clearingNode = _clearingFactory.Create(node);
		_clearingContainer.AddChild(clearingNode);
		clearingNode.Translate(handledNodes.Count * new Vector3(4, 0, 0));
			
		handledNodes.Add(node);
		
		
		
		foreach (var neighbour in node.Neighbours)
		{
			AddSubtree(neighbour, handledNodes);
		}
		
		
	}
}
