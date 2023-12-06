using Archetype.Framework.Design;

namespace Archetype.Prototype1;

public class CardPool : IProtoCards
{
    private readonly Dictionary<string, IProtoCard> _cards = new();

    public CardPool()
    {
        
    }
    
    public IProtoCard? GetProtoCard(string name)
    {
        throw new NotImplementedException();
    }
}