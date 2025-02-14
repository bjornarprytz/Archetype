using Archetype.Framework.Core;

namespace Archetype.Framework.GameLoop;

public interface ICardPool
{
    IEnumerable<ICardProto> GetCards();
    ICardProto? GetCard(string cardName);
}