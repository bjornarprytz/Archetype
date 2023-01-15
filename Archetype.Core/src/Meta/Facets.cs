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

public interface IPower
{
    [Description("Power")]
    int Power { get; set; }
}

public interface ISlots
{
    [Description("Slots")]
    int Slots { get; set; }
}

public interface IMovement
{
    [Description("Movement")]
    int Movement { get; set; }
}

public interface ICost
{
    [Description("Cost")]
    int Cost { get; set; }
}

public interface IValue
{
    [Description("Value")]
    int Value { get; set; }
}

public interface IType
{
    [Description("Type")]
    CardType Type { get; }
}

public interface ITags
{
    [Description("Tags")]
    IEnumerable<string> Tags { get; }
    void AddTag(string tag);
    void RemoveTag(string tag);
}