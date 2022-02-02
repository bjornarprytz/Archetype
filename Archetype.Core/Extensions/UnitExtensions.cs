using Archetype.Core.Atoms.Base;

namespace Archetype.Core.Extensions;

public static class UnitExtensions
{
    public static bool IsDead(this IUnit unit) => unit.Health <= 0;
}