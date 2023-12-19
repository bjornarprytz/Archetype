using Archetype.Framework.Core.Primitives;
using Archetype.Framework.State;

namespace Archetype.Framework.BaseRules.Keywords.Primitive;

public abstract class Target<TAtom> : TargetSpecificationDefinition
    where TAtom : IAtom
{
    public override IEnumerable<IAtom> GetAtoms(IResolutionContext context, IKeywordInstance keywordInstance)
    {
        return context.GameState.Atoms.Values.OfType<TAtom>().Cast<IAtom>().Where(a => Filter(a, context, keywordInstance)).ToList();
    }
}