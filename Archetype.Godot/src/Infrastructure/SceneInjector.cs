using System.Collections.Generic;
using Godot;
using Archetype.Godot.Extensions;

namespace Archetype.Godot.Infrastructure;

public class SceneInjector : Node
{
	public override void _EnterTree()
	{
		foreach (var child in this.GetSubtree<Node>())
		{
			child.ResolveDependencies();
		}
	}
}
