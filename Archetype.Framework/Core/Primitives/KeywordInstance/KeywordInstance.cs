using Archetype.Framework.State;

namespace Archetype.Framework.Core.Primitives;

public interface IKeywordInstance
{
    Guid Id { get; }
    string ResolveFuncName { get; }
    IReadOnlyList<IKeywordOperand> Operands { get; }
}

public record KeywordInstance : IKeywordInstance
{
    public Guid Id { get; } = Guid.NewGuid();
    public required string ResolveFuncName { get; init; }
    public IReadOnlyList<IKeywordOperand> Operands { get; init; } = new List<KeywordOperand>();
}
