using Archetype.Prototype1Data;

namespace Archetype.Godot.Infrastructure;

public interface IClearingFactory
{
    Clearing Create(IMapNode mapNode);
}

public class ClearingFactory : IClearingFactory
{
    private readonly ISceneFactory _sceneFactory;

    public ClearingFactory(ISceneFactory sceneFactory)
    {
        _sceneFactory = sceneFactory;
    }
    
    public Clearing Create(IMapNode mapNode)
    {
        var clearingNode = _sceneFactory.CreateNode<Clearing>();
        
        clearingNode.Load(mapNode);

        return clearingNode;
    }
}