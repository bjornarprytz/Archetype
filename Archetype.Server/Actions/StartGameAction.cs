using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Unit = MediatR.Unit;

namespace Archetype.Server.Actions
{
    public class StartGameAction : IRequest
    {
        public IEnumerable<string> DeckList { get; }
        
        public string HQStructureName { get; }
        public Guid HQPlacement { get; }

        public StartGameAction(IEnumerable<string> deckList, string hqStructureName, Guid hqPlacement)
        {
            DeckList = deckList;
            HQStructureName = hqStructureName;
            HQPlacement = hqPlacement;
        }
    }
    
    public class StartGameActionHandler : IRequestHandler<StartGameAction>
    {
        public Task<Unit> Handle(StartGameAction request, CancellationToken cancellationToken)
        {
            

            return Unit.Task;
        }
    }
}