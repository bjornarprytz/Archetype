using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.State;

namespace Archetype.Framework.Proto;

public interface IKeywordInstance
{
    string Keyword { get; }
    IReadOnlyList<KeywordOperand> Operands { get; }
    IReadOnlyList<KeywordTarget> Targets { get; }
}

public interface ICompositeKeywordInstance : IKeywordInstance
{
    IReadOnlyList<IKeywordInstance> Children { get; }
}

public record CompositeKeywordInstance : KeywordInstance, ICompositeKeywordInstance
{
    public IReadOnlyList<IKeywordInstance> Children { get; init; } = new List<IKeywordInstance>();
}


public record KeywordInstance : IKeywordInstance
{
    private readonly string _keyword = "_UNINITIALIZED_";
    
    public string Keyword
    {
        get => _keyword;
        init => _keyword = value.ToUpper();
    }

    public IReadOnlyList<KeywordOperand> Operands { get; init; } = new List<KeywordOperand>();
    
    public IReadOnlyList<KeywordTarget> Targets { get; init; } = new List<KeywordTarget>();
}

public record KeywordTarget(
    Func<IResolutionContext, IAtom> GetTarget
);

public record KeywordOperand
{
    public Func<IResolutionContext, object> GetValue { get; init; }
}
