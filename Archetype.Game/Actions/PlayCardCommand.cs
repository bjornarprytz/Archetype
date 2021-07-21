using Archetype.Core;
using MediatR;

namespace Archetype.Game
{
    public class PlayCardCommand : IRequest<bool>
    {
        public CardData Card { get; set; }
    }
}
