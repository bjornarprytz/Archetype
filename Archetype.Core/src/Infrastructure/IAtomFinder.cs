namespace Archetype.Core.Atoms.Infrastructure;

public interface IAtomFinder
{
    public IAtom FindAtom(Guid id);
    public T FindAtom<T>(Guid id) where T : IAtom;
}