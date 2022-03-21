using Archetype.Godot.Infrastructure;
using Archetype.Prototype1Data;

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
        var clearingNode = _sceneFactory.CreateNode<ClearingNode>();
        
        clearingNode.Load(mapNode);

        return clearingNode;
    }
}