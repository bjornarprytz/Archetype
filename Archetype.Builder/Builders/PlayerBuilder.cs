using System.Collections.Generic;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Builder
{
    public class PlayerBuilder : IBuilder<IPlayerProtoData>
    {
        private readonly List<ICardProtoData> _deckList = new();

        private readonly PlayerProtoData _playerProtoData;
        
        internal PlayerBuilder()
        {
            _playerProtoData = new PlayerProtoData(_deckList);
        }

        public PlayerBuilder Add(ICardProtoData card)
        {
            _deckList.Add(card);

            return this;
        }
        
        public IPlayerProtoData Build()
        {
            return _playerProtoData;
        }
    }
}