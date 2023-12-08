using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;

namespace Archetype.Framework.State;

public class Hand : Zone
{
    public override IReadOnlyDictionary<string, IKeywordInstance> Characteristics { get; } = 
        Declare.Characteristics(
            ("TYPE", "hand")
        );
}