using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.State;

namespace Archetype.Framework.Proto;

public record KeywordInstance
{
    public string Keyword { get; init; }
    public IReadOnlyList<KeywordOperand> Operands { get; init; }
    
    public IReadOnlyList<KeywordTarget> Targets { get; init; }
}

public record KeywordTarget(
    Func<IResolutionContext, IAtom> GetTarget
);

public record KeywordOperand
{
    public Func<IResolutionContext, object> GetValue { get; init; }
}
