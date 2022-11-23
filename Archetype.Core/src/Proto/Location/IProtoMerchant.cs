using Archetype.Core.Proto.PlayingCard;

namespace Archetype.Core.Proto.Location;

public interface IProtoMerchant : IProtoLocation
{
    public IEnumerable<IProtoPlayingCard> CardsForSale { get; }
}