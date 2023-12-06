using System.Collections;
using Archetype.Framework.Extensions;
using Archetype.Framework.State;

namespace Archetype.Framework.Core.Primitives;

public record CardTargetDescription(IAtomProvider Filter, bool IsOptional);