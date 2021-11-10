using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Archetype.Game.Actions
{
    public class StartGameAction : IRequest
    {
        public IEnumerable<Guid> DeckList { get; }

        public StartGameAction(IEnumerable<Guid> deckList)
        {
            DeckList = deckList;
        }
    }
    
    public class StartGameActionHandler : IRequestHandler<StartGameAction>
    {
        public StartGameActionHandler()
        {
            
        }
        
        public Task<Unit> Handle(StartGameAction request, CancellationToken cancellationToken)
        {
            // TODO: Set up the game state ready to play
            
            throw new NotImplementedException();
        }
    }
}