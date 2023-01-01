using Archetype.Core.Effects;
using Archetype.Core.Infrastructure;

namespace Archetype.Rules.Actions;

public record ActionResult(IEnumerable<IResult> Results) : IActionResult;
