using Archetype.Framework.Design;
using Archetype.Framework.State;

namespace Archetype.Framework.Core.Primitives;



public interface IActionBlock
{
    IAtom Source { get; }
    IReadOnlyList<IKeywordInstance> TargetsDescriptors { get; }
    IReadOnlyList<IKeywordInstance> Effects { get; }
    IReadOnlyList<IKeywordInstance> AfterEffects { get; }
    IReadOnlyList<IKeywordInstance> Costs { get; }
    IReadOnlyList<IKeywordInstance> Conditions { get; }
    IReadOnlyList<int> ComputedValues { get; }
    
    void UpdateComputedValues(IRules rules, IResolutionContext resolutionContext);
}


