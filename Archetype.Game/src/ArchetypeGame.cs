using Archetype.Core.Infrastructure;
using Archetype.Game.State;
using Archetype.Rules;
using Archetype.Rules.Encounter;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Archetype.Game;

public interface IArchetypeGame
{
    public IGameState State { get; }
    
    public Task PlayCard(PlayCard.Command command);
    public Task EndTurn(EndTurn.Command command);
}

internal class ArchetypeGame : IArchetypeGame
{
    public IGameState State { get; }
    private readonly IMediator _mediator;

    private ArchetypeGame(IGameState state)
    {
        State = state;
        var container = new ServiceCollection();
        
        container
            .AddSingleton(Static.Random)
            .AddSingleton(State)
            .AddRules();
        
        var serviceProvider = container.BuildServiceProvider();
        _mediator = serviceProvider.GetRequiredService<IMediator>();
    }
    
    public static IArchetypeGame Create(int seed)
    {
        var random = Static.SetRandomSeed(seed);
        
        var initialState = GameState.Init(random);
        
        return new ArchetypeGame(initialState);
    }

    public static IArchetypeGame Load(IGameState gameState, int seed)
    {
        return new ArchetypeGame(gameState);
    }
    
    public Task PlayCard(PlayCard.Command command)
    {
        return _mediator.Send(command);
    }

    public Task EndTurn(EndTurn.Command command)
    {
        return _mediator.Send(command);
    }
}