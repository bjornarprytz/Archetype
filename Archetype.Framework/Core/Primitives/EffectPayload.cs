using Archetype.Framework.State;

namespace Archetype.Framework.Core.Primitives;

public record EffectPayload(Guid Id, IAtom Source, string EffectId, IReadOnlyList<object?> Operands);


