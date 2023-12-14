using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;
using Archetype.Framework.Meta;

namespace Archetype.Framework.BaseRules.Keywords.Primitive;

[ComputedValueKeyword("COMPUTE_MAX", typeof(OperandDeclaration<IAtomProvider, string>))]
public class ComputeMax : ComputedValueDefinition
{
    public override string ReminderText => "Compute the maximum value of the given characteristic across all targets.";

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

[ComputedValueKeyword("COMPUTE_MIN", typeof(OperandDeclaration<IAtomProvider, string>))]
public class ComputeMin : ComputedValueDefinition
{
    public override string ReminderText => "Compute the minimum value of the given characteristic across all targets.";

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

[ComputedValueKeyword("COMPUTE_SUM", typeof(OperandDeclaration<IAtomProvider, string>))]
public class ComputeSum : ComputedValueDefinition
{
    public override string ReminderText => "Compute the sum of the given characteristic across all targets.";

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

[ComputedValueKeyword("COMPUTE_COUNT", typeof(OperandDeclaration<IAtomProvider, string>))]
public class ComputeCount : ComputedValueDefinition
{
    public override string ReminderText => "Compute the number of targets that have the given characteristic.";

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