using Archetype.Framework.State;

namespace Archetype.Framework.Core.Primitives;

public record PromptRef<TAtom>(Guid PromptId) : KeywordOperand<IReadOnlyList<TAtom>>(ctx =>
    ctx?.PromptResponses[PromptId].Selection.Cast<TAtom>().ToList() ?? throw new InvalidOperationException(
        $"Cannot access Prompt with id ({PromptId}) from context ({ctx})."))
    where TAtom : IAtom;
    