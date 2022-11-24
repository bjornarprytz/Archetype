using Archetype.Rules.Encounter;
using MediatR;

namespace Archetype.Game;

public interface IEncounterGame
{
    public Task PlayCard(PlayCard.Command command);
    public Task EndTurn(EndTurn.Command command);
}

internal class EncounterGame : IEncounterGame
{
    private readonly IMediator _mediator;

    public EncounterGame(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task PlayCard(PlayCard.Command command)
    {
        var affected = await _mediator.Send(command);

        foreach (var guid in affected)
        {
            Console.WriteLine($"From C#: Atom {guid} was affected");
        }
        
        return;
    }

    public Task EndTurn(EndTurn.Command command)
    {
        return _mediator.Send(command);
    }
}