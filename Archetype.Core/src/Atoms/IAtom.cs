namespace Archetype.Core.Atoms;

public interface IAtom
{
    public IAtom Owner { get; }
    public Guid Id { get; }
}