using System.Diagnostics;
using Archetype.Godot.Infrastructure;
using Archetype.Prototype1Data;
using Godot;

namespace Archetype.Godot.Clearing;


public interface IClearingFactory
{
    ClearingNode Create(IMapNode mapNode);
}

public class ClearingFactory : IClearingFactory
{
    private readonly ISceneFactory _sceneFactory;

    public ClearingFactory(ISceneFactory sceneFactory)
    {
        _sceneFactory = sceneFactory;
    }
    
    public ClearingNode Create(IMapNode mapNode)
    {
        var sw = new Stopwatch();
        sw.Start();
        var clearingNode = _sceneFactory.CreateNode<ClearingNode>();
        sw.Stop();
        GD.Print($"Spent {sw.ElapsedMilliseconds} ms creating clearing");
        
        clearingNode.Load(mapNode);

        return clearingNode;
    }
}