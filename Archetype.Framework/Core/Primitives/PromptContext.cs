using Archetype.Framework.State;

namespace Archetype.Framework.Core.Primitives;

public interface IPromptContext
{
    Guid PromptId { get; }
    IReadOnlyList<IAtom> Selection { get; }
}

public record PromptContext(Guid PromptId, IReadOnlyList<IAtom> Selection) : IPromptContext;