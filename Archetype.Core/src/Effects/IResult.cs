namespace Archetype.Core.Effects;

public interface IResult
{
    public IEnumerable<Guid> AffectedAtoms { get; }
}