using Archetype.Play.Factory;

namespace Archetype.Play.Context;


public interface IGameContext
{
    // State
    
    
    // Actions
    ISetupContext Setup();
    IDeckBuilderContext BuildDeck();
}

internal class GameContext : IGameContext
{
    private readonly IFactory<ISetupContext> _setupContextFactory;
    private readonly IFactory<IDeckBuilderContext> _deckBuilderContextFactory;


    public GameContext(
        IFactory<ISetupContext> setupContextFactory,
        IFactory<IDeckBuilderContext> deckBuilderContextFactory)
    {
        _setupContextFactory = setupContextFactory;
        _deckBuilderContextFactory = deckBuilderContextFactory;
    }
    public ISetupContext Setup()
    {
        return _setupContextFactory.Create();
    }

    public IDeckBuilderContext BuildDeck()
    {
        return _deckBuilderContextFactory.Create();
    }
}