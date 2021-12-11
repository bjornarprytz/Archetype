using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Extensions
{
    public static class UnitExtensions
    {
        public static bool IsDead(this IUnit unit) => unit.Health <= 0;
    }
}