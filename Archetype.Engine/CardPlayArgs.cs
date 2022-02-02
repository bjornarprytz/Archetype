namespace Archetype.Engine;

public interface ICardPlayArgs
{
    Guid CardGuid { get; }
    Guid WhenceGuid { get; }
    IEnumerable<Guid> TargetGuids { get; }
}