using Archetype.Core.Atoms.Zones;

namespace Archetype.Core.Meta;

public interface IZoned
{
    IZone? CurrentZone { get; set; }
}

public interface IHealth
{
    [Description("Health")]
    int CurrentHealth { get; set; }
    [Description("Max Health")]
    int MaxHealth { get; set; }
}