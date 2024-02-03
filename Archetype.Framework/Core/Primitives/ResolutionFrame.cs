namespace Archetype.Framework.Core.Primitives;

public interface IResolutionFrame
{
    IResolutionContext Context { get; }
    IReadOnlyList<IKeywordInstance> Effects { get; }
}

public record ResolutionFrame(IResolutionContext Context, IReadOnlyList<IKeywordInstance> Effects) : IResolutionFrame;