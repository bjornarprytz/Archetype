using System.Reflection;
using Archetype.Framework.Core;

namespace Archetype.Framework.GameLoop;

public interface IRules
{
    IEnumerable<IKeyword> GetKeywords();
    IKeyword? GetKeyword(string keyword);
    
    IEnumerable<ICardProto> GetCardPool();
    ICardProto? GetCard(string cardName);
}

public interface IKeyword
{
    public string Keyword { get; }
    public MethodInfo Method { get; }
}
