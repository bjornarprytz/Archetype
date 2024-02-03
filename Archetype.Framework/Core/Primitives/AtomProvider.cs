using Archetype.Framework.State;

namespace Archetype.Framework.Core.Primitives;

public class AtomProvider(Func<IResolutionContext, IEnumerable<IAtom>> scopeFunc, 
    params Func<IResolutionContext, IAtom, bool>[] filters) : IAtomProvider
{
    public IEnumerable<IAtom> ProvideAtoms(IResolutionContext context) => 
        scopeFunc(context)
            .Where(a => 
                filters.All(f => f(context, a)));
}

public interface IAtomProvider
{
    public IEnumerable<IAtom> ProvideAtoms(IResolutionContext context);
}