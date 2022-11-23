using Archetype.Core.Atoms;

namespace Archetype.Core.Infrastructure;

public interface IAtomFinder
{
    public IAtom FindAtom(Guid id);
    public T FindAtom<T>(Guid id) where T : IAtom;
}