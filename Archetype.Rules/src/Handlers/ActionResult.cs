using Archetype.Core.Effects;
using Archetype.Core.Infrastructure;

namespace Archetype.Rules.Encounter;

public record ActionResult(IEnumerable<IResult> Results) : IActionResult;
