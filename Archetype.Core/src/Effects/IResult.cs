namespace Archetype.Core.Effects;

public interface IResult
{
    // TODO: Do we need this?
    
    public IEnumerable<Guid> AffectedAtoms { get; }
}