using System.Reactive.Subjects;
using Archetype.Prototype2Data.Base;
using Archetype.Prototype2Data.Cards;
using Archetype.Prototype2Data.Zones;

namespace Archetype.Prototype2Data.GameGraph;

internal class GameView : IGameView
{
    private readonly IGameState _gameState;

    public GameView(IGameState gameState)
    {
        _gameState = gameState;
    }


    public IGameStateView GameState => _gameState;

    public void StartGame()
    {
        _gameState.Player.DrawBuilding();
        _gameState.Player.DrawBuilding();
        _gameState.Player.DrawCrew();
        
        Upkeep();
    }

    public void PlayBuilding(PlayBuildingArgs args)
    {
        
    }

    public void PlayCrewToTask(CrewToTaskArgs args)
    {
        
    }
    
    public void PlayCrewToDefend(CrewToDefendArgs args)
    {
        
    }

    public void EndTurn()
    {
        PreCombatEffects();
        Combat();
        Upkeep();
    }

    private void PreCombatEffects()
    {
        // TODO: Resolve some special pre-combat effects maybe?
        
    }

    private void Combat()
    {
        foreach (var clearing in _gameState.EachEncounter())
        {
            var defense = clearing.TotalMorale();
            var attack = clearing.TotalFear();


            if (defense == attack)
            {
                continue;
            }
            else if (defense > attack)
            {
                clearing.FightBack();
            }
            else if (2 * defense < attack)
            {
                clearing.Rout();
            }
            else if (defense < attack)
            {
                clearing.Damage();
            }
        }
    }

    private void Upkeep()
    {
        // TODO: Complete buildings
        // TODO: Draw enemy cards
        // TODO: Draw building and/or crew cards
    }
}

internal class GameState : IGameState
{
    public GameState(IPlayer player, IMap map, IEnumerable<IEnemy> enemyCards)
    {
        Player = player;
        Map = map;
        
        EnemyDeck.AddCards(enemyCards);
    }

    public IPlayer Player { get; }
    IPlayerView IGameStateView.Player => Player;
    
    public IMap Map { get; }
    IMapView IGameStateView.Map => Map;

    public IEnemyDeck EnemyDeck { get; } = new EnemyDeck();
    IEnemyDeckView IGameStateView.EnemyDeck => EnemyDeck;
}

internal class Player : IPlayer
{
    private readonly Subject<IGameAtom> _cardDrawn = new ();
    private readonly Subject<IGameAtom> _cardRemoved = new ();

    public Player(IEnumerable<IBuilding> buildings, IEnumerable<ICrewView> crew, IEnumerable<IBuilding> stations)
    {
        BuildingDeck.AddCards(buildings);
        CrewDeck.AddCards(crew);
        StationStack.AddBuildings(stations);
    }

    IZoneView<IGameAtom> IPlayerView.Hand => Hand;
    public ICrewDeck CrewDeck { get; } = new CrewDeck();
    public IBuildingDeck BuildingDeck { get; } = new BuildingDeck();
    public IStationStack StationStack { get; } = new StationStack();
    public void DrawBuilding()
    {
        Hand.Add(BuildingDeck.Draw());
    }

    public void DrawCrew()
    {
        Hand.Add(CrewDeck.Draw());
    }

    public IZone<IGameAtom> Hand { get; } = new Zone<IGameAtom>();

    ICrewDeckView IPlayerView.CrewDeck => CrewDeck;

    IBuildingDeckView IPlayerView.BuildingDeck => BuildingDeck;

    IStationStackView IPlayerView.StationStack => StationStack;
}

internal class Map : IMap
{
    private readonly List<IMapNode> _nodes = new();
    
    public Map(IEnumerable<IMapNode> nodes)
    {
        _nodes.AddRange(nodes);

        Root = _nodes.FirstOrDefault();
    }
    
    public IEnumerable<IMapNode> Nodes => _nodes;
    IEnumerable<IMapNodeView> IMapView.Nodes => _nodes;

    public IMapNode Root { get; }
    IMapNodeView IMapView.Root => Root;
}

