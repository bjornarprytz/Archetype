using Archetype.Game.Payloads.Atoms.Base;

namespace Archetype.Game.Extensions
{
    internal static class UnitExtensions
    {
        public static bool IsDead(this IUnit unit) => unit.Health <= 0;
    }
}