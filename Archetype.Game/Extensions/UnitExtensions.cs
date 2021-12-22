using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Extensions
{
    internal static class UnitExtensions
    {
        public static bool IsDead(this IUnit unit) => unit.Health <= 0;
    }
}