using Archetype.Play.Factory;

namespace Archetype.Play.Context;


public interface IGameContext
{
    // State
        
    // Actions
    ISetupContext Setup();
}

internal class GameContext : IGameContext
{
    private readonly IFactory<ISetupContext> _setupContextFactory;
    

    public GameContext(IFactory<ISetupContext> setupContextFactory)
    {
        _setupContextFactory = setupContextFactory;
    }
    public ISetupContext Setup()
    {
        return _setupContextFactory.Create();
    }
}