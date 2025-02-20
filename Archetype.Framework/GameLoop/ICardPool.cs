using Archetype.Framework.Core;
using Archetype.Framework.State;

namespace Archetype.Framework.GameLoop;

public interface ICardPool
{
    IEnumerable<ICardProto> GetCards();
    
    [PathPart("card")]
    ICardProto? GetCard(string cardName);
}