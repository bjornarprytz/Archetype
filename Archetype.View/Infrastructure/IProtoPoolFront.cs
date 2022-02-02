namespace Archetype.View.Infrastructure;

public interface IProtoPoolFront
{
    IEnumerable<ISetFront> Sets { get; }
}