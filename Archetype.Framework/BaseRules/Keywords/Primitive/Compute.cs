using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;
using Archetype.Framework.Meta;

namespace Archetype.Framework.BaseRules.Keywords.Primitive;

public class ComputeMax : ComputedValueDefinition
{
    protected override OperandDeclaration<IAtomProvider, string> OperandDeclaration { get; } = new();
    public override int Compute(IResolutionContext context, IKeywordInstance keywordInstance)
    {
        var (atomProvider, characteristic) = OperandDeclaration.Unpack(keywordInstance);

        var atoms = atomProvider.ProvideAtoms(context).ToList();
        
        return atoms.Count == 0 ? 0 
            : atoms
                .Where(t => t.HasCharacteristic(characteristic))
                .Max(a => a.GetCharacteristicValue(characteristic, context));
    }
}

public class ComputeMin : ComputedValueDefinition
{
    protected override OperandDeclaration<IAtomProvider, string> OperandDeclaration { get; } = new();
    public override int Compute(IResolutionContext context, IKeywordInstance keywordInstance)
    {
        var (atomProvider, characteristic) = OperandDeclaration.Unpack(keywordInstance);

        var atoms = atomProvider.ProvideAtoms(context).ToList();
        
        return atoms.Count == 0 ? 0 
            : atoms
                .Where(t => t.HasCharacteristic(characteristic))
                .Min(a => a.GetCharacteristicValue(characteristic, context));
    }
}

public class ComputeSum : ComputedValueDefinition
{
    protected override OperandDeclaration<IAtomProvider, string> OperandDeclaration { get; } = new();
    public override int Compute(IResolutionContext context, IKeywordInstance keywordInstance)
    {
        var (atomProvider, characteristic) = OperandDeclaration.Unpack(keywordInstance);

        var atoms = atomProvider.ProvideAtoms(context).ToList();
        
        return atoms.Count == 0 ? 0 
            : atoms
                .Where(t => t.HasCharacteristic(characteristic))
                .Sum(a => a.GetCharacteristicValue(characteristic, context));
    }
}

public class ComputeCount : ComputedValueDefinition
{
    protected override OperandDeclaration<IAtomProvider, string> OperandDeclaration { get; } = new();
    public override int Compute(IResolutionContext context, IKeywordInstance keywordInstance)
    {
        var (atomProvider, characteristic) = OperandDeclaration.Unpack(keywordInstance);

        var atoms = atomProvider.ProvideAtoms(context).ToList();
        
        return atoms.Count == 0 ? 0 
            : atoms
                .Count(a => a.HasCharacteristic(characteristic));
    }
}