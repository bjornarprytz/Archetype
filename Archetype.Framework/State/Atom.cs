using Archetype.Framework.Core.Primitives;

namespace Archetype.Framework.State;

public interface IAtom
{
    Guid Id { get; }
    IReadOnlyDictionary<string, IKeywordInstance> Characteristics { get; }
    IDictionary<string, object> State { get; }
    IZone? CurrentZone { get; set; }
}


public abstract class Atom : IAtom
{
    public Guid Id { get; } = Guid.NewGuid();
    public abstract IReadOnlyDictionary<string, IKeywordInstance> Characteristics { get; }
    public IDictionary<string, object> State { get; } = new Dictionary<string, object>();
    
    public IZone? CurrentZone { get; set; }
}