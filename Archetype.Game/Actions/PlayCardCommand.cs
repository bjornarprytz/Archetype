using Archetype.Core;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Archetype.Game
{
    public class PlayCardCommand : IRequest<bool>
    {
        public ICard Card { get; set; }

        public ICardArgs Args { get; set; }
    }

    public class PlayCardCommandHandler : IRequestHandler<PlayCardCommand, bool>
    {
        private readonly IGameState _gameState;

        public PlayCardCommandHandler(IGameState gameState)
        {
            _gameState = gameState;
        }

        public async Task<bool> Handle(PlayCardCommand request, CancellationToken cancellationToken)
        {
            if (!request.Card.ValidateArgs(request.Args, _gameState))
                return false;

            await request.Card.ResolveAsync(request.Args, _gameState);

            return true;
        }
    }
}
