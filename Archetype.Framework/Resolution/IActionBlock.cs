using Archetype.Framework.Core;
using Archetype.Framework.State;

namespace Archetype.Framework.Resolution;

public interface IActionBlock
{
    public IAtom Source { get; }
    public IReadOnlyDictionary<string, IValue<int?>> Costs { get; }
    public IEnumerable<TargetProto> Targets { get; }
    public IEnumerable<EffectProto> Effects { get; }
}