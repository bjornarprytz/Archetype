using Archetype.Core;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Archetype.Game.Infrastructure;

namespace Archetype.Game
{
    public class PlayCardCommand : IRequestWrapper
    {
        public IPlayer Player { get; set; }
        public ICard Card { get; set; }
        public ICardArgs Args { get; set; }
    }

    public class PlayCardCommandHandler : IHandlerWrapper<PlayCardCommand>
    {
        private readonly ICardStack _cardStack;

        public PlayCardCommandHandler(ICardStack cardStack)
        {
            _cardStack = cardStack;
        }

        public Task<IResponseWrapper> Handle(PlayCardCommand request, CancellationToken cancellationToken)
        {
            _cardStack.Push(request.Card, request.Args);

            return Task.FromResult<IResponseWrapper>(Response.Ok());
        }
    }
}
