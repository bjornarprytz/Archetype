namespace Archetype.Framework.Core.Primitives;

public interface IResolutionFrame
{
    IResolutionContext Context { get; }
    IReadOnlyList<IKeywordInstance> Costs { get; }
    IReadOnlyList<IKeywordInstance> Effects { get; }
}

public record ResolutionFrame(IResolutionContext Context, IReadOnlyList<IKeywordInstance> Costs,
    IReadOnlyList<IKeywordInstance> Effects) : IResolutionFrame;