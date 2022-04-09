using Archetype.Prototype2Data.Base;
using Archetype.Prototype2Data.Cards;
using Archetype.Prototype2Data.Zones;

namespace Archetype.Prototype2Data;

public record BuildingData(string Name, int Cost, int HitPoints, int Morale, IReadOnlyList<BuildingType> Types, IReadOnlyList<BuildingKeyword> Keywords);
public record CrewData(string Name, int Resources, int Morale, IReadOnlyList<CrewType> Types, IReadOnlyList<CrewKeyword> Keywords);
public record EnemyData(string Name, int Fear, Size Size, IReadOnlyList<EnemyType> Types, IReadOnlyList<EnemyKeyword> Keywords);

public record PlayBuildingArgs(IBuildingView Building, IMapNodeView Target, ICrewView Payment);
public record CrewToTaskArgs(ICrewView CrewView, IBuildingView Target);
public record CrewToDefendArgs(ICrewView CrewView, IMapNodeView Target);

public record ZoneChange<T>(IZoneView<T>? From, IZoneView<T>? To) where T : IGameAtom;

public enum Size
{
    Small,
    Medium,
    Large
}

public enum EnemyType
{
    Woods,
    Sea,
    Undead,
}

public enum EnemyKeyword
{
    Weakness,
}

public enum CrewType
{
    Security,
    Worker,
    Clergy
}

public enum CrewKeyword
{
    Bait,
    Repair
}

public enum BuildingType
{
    Economy,
    Stronghold,
    Station
}

public enum BuildingKeyword
{
    Draw,
    ClearCutting,
    Ranged,
    Repair,
    RaiseDead,
}