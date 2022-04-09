using Archetype.Prototype2Data.Base;
using Archetype.Prototype2Data.Cards;
using Archetype.Prototype2Data.Zones;

namespace Archetype.Prototype2Data.GameGraph;

public interface IGameView
{
    IGameStateView GameState { get; }

    void PlayBuilding(PlayBuildingArgs args);
    void PlayCrewToTask(CrewToTaskArgs args);
    void PlayCrewToDefend(CrewToDefendArgs args);
    
    void StartGame();
    void EndTurn();
}

public interface IGameStateView
{
    IPlayerView Player { get; }
    IMapView Map { get; }
    IEnemyDeckView EnemyDeck { get; }
}

internal interface IGameState : IGameStateView
{
    new IPlayer Player { get; }
    new IMap Map { get; }
    new IEnemyDeck EnemyDeck { get; }
}

public interface IPlayerView
{
    IZoneView<IGameAtom> Hand { get; }
    
    ICrewDeckView CrewDeck { get; }
    IBuildingDeckView BuildingDeck { get; }
    IStationStackView StationStack { get; }
}

internal interface IPlayer : IPlayerView
{
    new IZone<IGameAtom> Hand { get; }
    
    new ICrewDeck CrewDeck { get; }
    new IBuildingDeck BuildingDeck { get; }
    new IStationStack StationStack { get; }
    void DrawBuilding();
    void DrawCrew();
}

public interface IMapView
{
    IEnumerable<IMapNodeView> Nodes { get; }
    IMapNodeView Root { get; }
}

internal interface IMap : IMapView
{
    new IEnumerable<IMapNode> Nodes { get; }
    new IMapNode Root { get; }
}