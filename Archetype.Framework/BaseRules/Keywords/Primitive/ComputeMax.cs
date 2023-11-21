using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.State;

namespace Archetype.BasicRules.Primitives;

public class ComputeMax : ComputedValueDefinition
{
    public override string Name => "COMPUTE_MAX";
    public override string ReminderText => "Compute the maximum value of the given property across all targets.";

    protected override OperandDeclaration<string> OperandDeclaration { get; } = new();
    public override int Compute(IResolutionContext context, IKeywordInstance keywordInstance)
    {
        var property = OperandDeclaration.UnpackOperands(keywordInstance);
        return keywordInstance.Targets.Select(t => t.GetTarget(context).State[property]).Cast<int>().Max();
    }
}