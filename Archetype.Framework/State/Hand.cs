using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;

namespace Archetype.Framework.State;

public class Hand : Zone
{
    public override IReadOnlyDictionary<string, int> Stats { get; } = new Dictionary<string, int>();

    public override IReadOnlyDictionary<string, string> Tags { get; } = new Dictionary<string, string>
    {
        {  "TYPE", "Hand" }
    };
}