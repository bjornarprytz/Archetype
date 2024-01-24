using Archetype.Framework.State;

namespace Archetype.Framework.Core.Primitives;

public record AtomProvider(Func<IResolutionContext, IEnumerable<IAtom>> ProvideAtomsFunc, Func<IAtom, IResolutionContext, bool> CheckAtomFunc ) : IAtomProvider
{
    public IEnumerable<IAtom> ProvideAtoms(IResolutionContext context) => ProvideAtomsFunc(context);

    public bool CheckAtom(IAtom atom, IResolutionContext context) => CheckAtomFunc(atom, context);
}

public interface IAtomProvider
{
    public IEnumerable<IAtom> ProvideAtoms(IResolutionContext context);

    public bool CheckAtom(IAtom atom, IResolutionContext context);
}