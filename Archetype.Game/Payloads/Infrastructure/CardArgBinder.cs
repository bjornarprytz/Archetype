using System.Collections.Generic;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Infrastructure
{
    public interface ICardArgBinder
    {
        ICardPlayArgs Bind(IPlayer player, ICard card, IMapNode whence, IEnumerable<IGameAtom> targets);
    }
    
    public class CardArgBinder
    {
        
    }
}