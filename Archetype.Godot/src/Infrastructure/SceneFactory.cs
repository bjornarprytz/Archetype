using System;
using System.Collections.Generic;
using Archetype.Godot.Extensions;
using Godot;

namespace Archetype.Godot.Infrastructure;

public interface IPackedSceneConfiguration
{
	T CreateInstance<T>() where T : Node;
}

public class PackedSceneConfiguration : IPackedSceneConfiguration
{
	private readonly Dictionary<Type, PackedScene> _packedScenes = new();

	private PackedSceneConfiguration() { }

	public static PackedSceneConfiguration Create(Action<PackedSceneConfiguration> builder)
	{
		var config = new PackedSceneConfiguration();
		
		builder?.Invoke(config);

		return config;
	} 
	
	internal PackedSceneConfiguration Add<T>(string scenePath)
	{
		var scene = ResourceLoader.Load<PackedScene>(scenePath) 
					?? throw new MissingPackedSceneException(scenePath);
		
		_packedScenes.Add(typeof(T), scene);

		return this;
	}

	public T CreateInstance<T>() where T : Node
	{
		var packedScene = _packedScenes[typeof(T)]
			?? throw new MissingPackedSceneException(typeof(T));

		var n = packedScene.Instance(); 
		
		if (n is not T node)
		{
			throw new MissingPackedSceneException(typeof(T), n.GetType());
		}

		return node;
	}
}

public interface ISceneFactory
{
	T CreateNode<T>() where T : Node;
}


public class SceneFactory : ISceneFactory
{
	private readonly IPackedSceneConfiguration _packedSceneConfiguration;
	
	public SceneFactory(IPackedSceneConfiguration packedSceneConfiguration)
	{
		_packedSceneConfiguration = packedSceneConfiguration;
	}

	public T CreateNode<T>()
		where T : Node
	{
		var node = _packedSceneConfiguration.CreateInstance<T>();

		node.ResolveDependencies();

		return node;
	}
}
