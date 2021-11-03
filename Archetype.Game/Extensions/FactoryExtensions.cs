using Archetype.Core;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Game.Extensions
{
    public static class FactoryExtensions
    {
        public static ICard CreateInstance(this ICardProtoData cardProtoData)
        {
            return new Card(cardProtoData); // TODO: Set zone, owner, etc?
        }
    }
}