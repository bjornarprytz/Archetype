using Archetype.Framework.Definitions;

namespace Archetype.BasicRules;

public static class TargetHelpers
{
    public static IEnumerable<TargetDescription> Required(params string[] filters)
    {
        return filters.Select(filter => new TargetDescription(Filter.Parse(filter), false));
    }
    public static IEnumerable<TargetDescription> Optional(params string[] filters)
    {
        return filters.Select(filter => new TargetDescription(Filter.Parse(filter), true));
    }
}