using Archetype.Framework.State;

namespace Archetype.Framework.Core.Primitives;

public interface IKeywordInstance
{
    Guid Id { get; }
    string Keyword { get; }
    IReadOnlyList<KeywordOperand> Operands { get; }
}

public record KeywordInstance : IKeywordInstance
{
    public Guid Id { get; } = Guid.NewGuid();
    public required string Keyword { get; init; } = null!;
    public IReadOnlyList<KeywordOperand> Operands { get; init; } = new List<KeywordOperand>();
}
