using Archetype.Prototype2Data.Base;
using Archetype.Prototype2Data.Cards;
using Archetype.Prototype2Data.GameGraph;
using Archetype.Prototype2Data.Zones;

namespace Archetype.Prototype2Data;

public class Generator
{
    public static IGameStateView Create()
    {
        return new GameState(
            new Player(InitialBuildingDeck(), InitialCrewDeck(), InitialStations()), 
            new Map(InitialMap()),
            EnemyDeck());
    }

    private static IEnumerable<IBuilding> InitialStations() =>
        new List<IBuilding>
        {
            MidWayStation,
            MidWayStation,
            MidWayStation,
            MidWayStation,
        };
    
    private static IEnumerable<IBuilding> InitialBuildingDeck() =>
        new List<IBuilding>
        {
            Lodge,
            Lodge,
            Lodge,
            Lodge,
            Lodge,
            Garrison,
            Garrison,
            Garrison,
            Garrison,
        };
    
    private static IEnumerable<ICrew> InitialCrewDeck() =>
        new List<ICrew>
        {
            Slusk,
            Slusk,
            Slusk,
            Slusk,
            Slusk,
            Slusk,
            Slusk,
            Slusk,
            Captain,
            Priest,
        };

    private static IEnumerable<IMapNode> InitialMap()
    {
        var a = new MapNode(2);
        var b = new MapNode(3);
        var c = new MapNode(1);
        var d = new MapNode(4);
        
        a.Connect(b);
        b.Connect(c);
        c.Connect(d);

        return new List<IMapNode> { a, b, c, d };
    }
    private static IEnumerable<IEnemy> EnemyDeck() =>
        new List<IEnemy>
        {
            Villvette,
            Villvette,
            Villvette,
            Villvette,
            Villvette,
            Villvette,
            Villvette,
            Villvette,
            Villvette,
            Villvette,
            Villvette
        };

    private static ICrew Slusk => new Crew(new CrewData("Slusk", 1, 1, new[] { CrewType.Worker }, new CrewKeyword[] { }));
    private static ICrew Captain => new Crew(new CrewData("Captain", 1, 2, new[] { CrewType.Security }, new CrewKeyword[] { }));
    private static ICrew Priest => new Crew(new CrewData("Priest", 1, 3, new[] { CrewType.Clergy }, new CrewKeyword[] { }));
    
    private static IEnemy Villvette => new Enemy(new EnemyData("Villvette", 2, Size.Small, new [] { EnemyType.Woods }, new EnemyKeyword[] { }));

    private static IBuilding Lodge => new Building(new BuildingData("Lodge", 1, 2, 3, new [] { BuildingType.Economy }, new BuildingKeyword[] { }));
    private static IBuilding Garrison => new Building(new BuildingData("Garrison", 1, 2, 3, new [] { BuildingType.Stronghold }, new BuildingKeyword[] { }));
    private static IBuilding MidWayStation => new Building(new BuildingData("Mid-way Station", 3, 5, 4, new [] { BuildingType.Station }, new BuildingKeyword[] { }));

}
