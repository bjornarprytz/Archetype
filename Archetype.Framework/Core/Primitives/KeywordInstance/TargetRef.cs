using Archetype.Framework.State;

namespace Archetype.Framework.Core.Primitives;

public record TargetSourceRef() : KeywordOperand<IAtom>(ctx => ctx?.Source ?? throw new InvalidOperationException(
    $"Cannot access Self from context ({ctx})."));

public record TargetRef(int TargetIndex) : TargetRef<IAtom>(TargetIndex);

public record TargetRef<TAtom>(int TargetIndex) : KeywordOperand<TAtom>(ctx =>
    ctx?.Targets[TargetIndex] as TAtom ?? throw new InvalidOperationException(
        $"Cannot access Target with index [{TargetIndex}] and type ({typeof(TAtom)}) from context ({ctx})."))
    where TAtom : class, IAtom;