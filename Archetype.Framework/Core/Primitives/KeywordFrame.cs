namespace Archetype.Framework.Core.Primitives;

public interface IKeywordFrame
{
    public IEvent Event { get; }
    public IReadOnlyList<IKeywordInstance> Effects { get; }
}

public record KeywordFrame(IEvent Event, IReadOnlyList<IKeywordInstance> Effects) : IKeywordFrame;