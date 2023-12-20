using Archetype.Framework.State;

namespace Archetype.Framework.Core.Primitives;

public abstract class TargetSpecificationDefinition : KeywordDefinition
{
    protected sealed override OperandDeclaration<bool> OperandDeclaration { get; } = new();

    public bool IsOptional(IResolutionContext context, IKeywordInstance keywordInstance)
    {
        var isOptional = OperandDeclaration.Unpack(keywordInstance, context);
        return isOptional;
    }
    public abstract bool Filter(IAtom atom, IResolutionContext context, IKeywordInstance keywordInstance);
    public abstract IEnumerable<IAtom> GetAtoms(IResolutionContext context, IKeywordInstance keywordInstance);
}