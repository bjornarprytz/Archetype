using Archetype.Framework.State;

namespace Archetype.Framework.Core.Primitives;

public interface IAtomProvider
{
    public IEnumerable<IAtom> ProvideAtoms(IResolutionContext context);

    public bool CheckAtom(IAtom atom, IResolutionContext context);
}