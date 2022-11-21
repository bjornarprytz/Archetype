namespace Archetype.Core.Proto;

public interface IProtoLocation : IProtoCard
{
    // Data about the location. Design TBD.
    
    public IEnumerable<string> Connections { get; } // e.g. "A-B", "B-C", "C-A"
}