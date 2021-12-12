using System.Collections.Generic;

namespace Archetype.Game.Payloads.Proto
{
    public interface IPlayerProtoData : IProtoData
    {
        IEnumerable<ICardProtoData> DeckList { get; }
        
    }
    
    public class PlayerProtoData : ProtoData, IPlayerProtoData
    {
        private readonly List<ICardProtoData> _deckList;

        public PlayerProtoData(List<ICardProtoData> deckList)
        {
            _deckList = deckList;
        }

        public IEnumerable<ICardProtoData> DeckList => _deckList;
    }
}