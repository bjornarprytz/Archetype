using System.Collections.Generic;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Game.Payloads.Pieces
{
    public interface ICardSet
    {
        string Name { get; set; }
        IEnumerable<ICardProtoData> Cards { get; }

        void AddCard(ICardProtoData cardData);
    }
}