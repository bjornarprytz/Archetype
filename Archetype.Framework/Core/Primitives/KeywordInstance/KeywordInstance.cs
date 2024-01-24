using Archetype.Framework.State;

namespace Archetype.Framework.Core.Primitives;

public interface IKeywordInstance
{
    Guid Id { get; }
    string ResolveFuncName { get; }
    IReadOnlyList<IKeywordOperand> Operands { get; }
}

public record KeywordInstance(string ResolveFuncName, params IKeywordOperand[] KeywordOperands) : IKeywordInstance
{
    public Guid Id { get; } = Guid.NewGuid();
    public IReadOnlyList<IKeywordOperand> Operands => KeywordOperands;
}
