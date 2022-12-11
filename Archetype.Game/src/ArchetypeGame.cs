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
    private readonly ServiceProvider _serviceProvider;

    private ArchetypeGame(IGameState state)
    {
        State = state;
        var container = new ServiceCollection();
        
        container
            .AddSingleton(new Random(state.Seed))
            .AddSingleton(State)
            .AddRules();
        
        _serviceProvider = container.BuildServiceProvider();
        _mediator = _serviceProvider.GetRequiredService<IMediator>();
    }
    
    public static IArchetypeGame Create(int seed)
    {
        var initialState = GameState.Init(seed);
        
        return new ArchetypeGame(initialState);
    }

    public static IArchetypeGame Load(IGameState gameState)
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