using Archetype.Framework.Core;
using Archetype.Framework.State;

namespace Archetype.Framework.GameLoop;

public interface ICardPool
{
    IEnumerable<ICardProto> GetCards();
    ICardProto? GetCard(string cardName);
}