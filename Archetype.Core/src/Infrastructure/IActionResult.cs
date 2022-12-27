using Archetype.Core.Effects;

namespace Archetype.Core.Infrastructure;

public interface IActionResult
{
    // This should carry information about state changes from game actions.
    
    IEnumerable<IResult> Results { get; }
}