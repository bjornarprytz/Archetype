using Archetype.Core;
using Archetype.Core.Proto.DeckBuilding;
using Archetype.Rules;
using Archetype.Rules.Encounter;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Archetype.Game;

public interface IArchetypeGame
{
    public IGameState State { get; }
    
    public Task<IEncounterGame> EnterLocation(Guid locationId);
    public Task DrawLocation();
    public Task<Guid> SaveDeck(IDeck deck);
    public Task ChooseDeck(Guid deckId);
    
    public Task<Guid> SaveGame();
    public Task QuitGame();
}

internal class ArchetypeGame : IArchetypeGame
{
    public IGameState State { get; }
    private readonly ServiceProvider _serviceProvider;

    private ArchetypeGame(IGameState state)
    {
        State = state;
        var container = new ServiceCollection();
        
        container
            .AddSingleton(State)
            .AddMediatR(typeof(PlayCard).Assembly);
        
        _serviceProvider = container.BuildServiceProvider();
    }
    
    public static IArchetypeGame Create(int seed)
    {
        var initialState = GameState.Init(seed);
        
        return new ArchetypeGame(initialState);
    }

    public static Task<IArchetypeGame> Load(Guid gameId)
    {
        // TODO: Load game state from database
        var initialState = GameState.Init(0);
        
        return Task.FromResult<IArchetypeGame>(new ArchetypeGame(initialState));
    }

    public Task<IEncounterGame> EnterLocation(Guid locationId)
    {
        throw new NotImplementedException();
    }

    public Task DrawLocation()
    {
        throw new NotImplementedException();
    }

    public Task<Guid> SaveDeck(IDeck deck)
    {
        throw new NotImplementedException();
    }

    public Task ChooseDeck(Guid deckId)
    {
        throw new NotImplementedException();
    }

    public Task<Guid> SaveGame()
    {
        throw new NotImplementedException();
    }

    public Task QuitGame()
    {
        throw new NotImplementedException();
    }
}