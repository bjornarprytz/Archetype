using Archetype.Core.Extensions;
using Archetype.Core.Infrastructure;
using Archetype.Rules;
using Microsoft.Extensions.DependencyInjection;

namespace Archetype.Game;

public interface IArchetypeGame
{
    public IGameState State { get; }
    
    public Task<IEncounterGame> EnterLocation(Guid locationId);
    public Task<IDeckBuilding> EnterDeckBuilding();
    public Task DrawLocation();
    
    
    public Task<Guid> SaveGame();
    public Task QuitGame();
}

internal class ArchetypeGame : IArchetypeGame
{
    public IGameState State { get; }
    private readonly IServiceProvider _serviceProvider;

    private ArchetypeGame(IGameState state)
    {
        State = state;
        var container = new ServiceCollection();
        
        container
            .AddSingleton(State)
            .AddTransient<IEncounterGame, EncounterGame>()
            .AddRules();
        
        _serviceProvider = container.BuildServiceProvider();
    }
    
    public static IArchetypeGame Create(int seed)
    {
        var initialState = GameState.Init(seed);

        MyCollectionExtensions.Random = new Random(seed); // TODO: Centralize random in another place
        
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

    public Task<IDeckBuilding> EnterDeckBuilding()
    {
        throw new NotImplementedException();
    }

    public Task DrawLocation()
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