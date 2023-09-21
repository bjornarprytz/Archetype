using Archetype.Framework.Definitions;

namespace Archetype.BasicRules;

public static class OperandHelpers
{
    public static IEnumerable<OperandDescription> Required(params KeywordOperandType[] types)
    {
        return types.Select(type => new OperandDescription(type, false));
    }
    public static IEnumerable<OperandDescription> Optional(params KeywordOperandType[] types)
    {
        return types.Select(type => new OperandDescription(type, true));
    }
}

public static class TargetHelpers
{
    public static IEnumerable<TargetDescription> Required(params string[] filters)
    {
        return filters.Select(filter => new TargetDescription(filter.ToCharacteristicFilter(), false));
    }
    public static IEnumerable<TargetDescription> Optional(params string[] filters)
    {
        return filters.Select(filter => new TargetDescription(filter.ToCharacteristicFilter(), true));
    }
}