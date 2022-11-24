using Archetype.Core.Effects;

namespace Archetype.Components;

public record Result(IEnumerable<Guid> AffectedAtoms) : IResult
{
    public static IResult Aggregate(IEnumerable<IResult> results) => new Result(results.SelectMany(r => r.AffectedAtoms));
}