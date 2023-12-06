using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Definitions;
using Archetype.Framework.Design;
using Archetype.Framework.State;

namespace Archetype.Prototype1;

public class BasicRules : IRules
{
    public IReadOnlyList<IPhase> TurnSequence { get; }
    public IKeywordDefinition? GetDefinition(string keyword)
    {
        throw new NotImplementedException();
    }

    public T? GetDefinition<T>() where T : IKeywordDefinition
    {
        throw new NotImplementedException();
    }
}