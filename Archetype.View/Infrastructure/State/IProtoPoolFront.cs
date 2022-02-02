namespace Archetype.View.Infrastructure.State;

public interface IProtoPoolFront
{
    IEnumerable<ISetFront> Sets { get; }
}