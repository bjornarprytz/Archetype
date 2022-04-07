using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;

namespace Archetype.Prototype2Data;

internal class GameView : IGameView
{
    private readonly GameState _gameState;

    public GameView(IGameState gameState)
    {
        _gameState = (GameState)gameState;
    }


    public IGameState GameState => _gameState;

    public void StartGame()
    {
        ((Player)_gameState.Player).Draw();
        ((Player)_gameState.Player).Draw();
        
        Upkeep();
    }

    public void PlayBuilding(IBuilding building, IMapNode mapNode, IEnumerable<ICrew> payment)
    {
        
    }

    public void PlayCrewToTask(ICrew crew, IBuilding building)
    {
        
    }
    
    public void PlayCrewToDefend(ICrew crew, IMapNode mapNode)
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
            var defense = clearing.TotalDefense();
            var attack = clearing.TotalAttack();


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
    public GameState(IPlayer player, IMap map, IWaveEmitter waveEmitter)
    {
        WaveEmitter = waveEmitter;
        Player = player;
        Map = map;
    }

    public IPlayer Player { get; }
    public IMap Map { get; }
    public IWaveEmitter WaveEmitter { get; }
}

internal class Player : IPlayer
{
    private readonly Subject<ICard> _cardDrawn = new Subject<ICard>();
    private readonly Subject<ICard> _cardRemoved = new Subject<ICard>();
    private readonly Stack<ICard> _deck = new Stack<ICard>();
    private readonly List<ICard> _hand = new List<ICard>();

    public Player(int resources, IEnumerable<ICard> deck)
    {
        Resources = resources;

        foreach (var card in deck.Shuffle())
        {
            _deck.Push(card);
        }
    }

    public int Resources { get; internal set; }
    public int CardsInDeck => _deck.Count;
    public IEnumerable<ICard> Hand => _hand;
    public IObservable<ICard> OnCardDrawn => _cardDrawn;
    public IObservable<ICard> OnCardRemoved => _cardRemoved;

    internal void Draw()
    {
        if (_deck.IsEmpty())
        {
            return;
        }

        var card = _deck.Pop();

        _hand.Add(card);
        
        _cardDrawn.OnNext(card);
    }
}

internal class Map : IMap
{
    private readonly List<IMapNode> _nodes = new List<IMapNode>();
    
    public Map(IEnumerable<IMapNode> nodes)
    {
        _nodes.AddRange(nodes);

        Root = _nodes.FirstOrDefault();
        StagingArea = _nodes.LastOrDefault();
    }

    public IEnumerable<IMapNode> Nodes => _nodes;
    public IMapNode? Root { get; }
    public IMapNode? StagingArea { get; }
}

internal class MapNode : IMapNode
{
    private readonly List<ICrew> _defenders = new List<ICrew>();
    private readonly List<IMapNode> _neighbours = new List<IMapNode>();

    private readonly int _baseExposure;
    
    public MapNode(int exposure)
    {
        Exposure = _baseExposure = exposure;
    }

    public int Exposure { get; }
    public IEnumerable<IMapNode> Neighbours => _neighbours;
    public IWave? Wave { get; }
    public IEnumerable<ICrew> Defenders => _defenders;
    public IBuilding? Building { get; internal set; }
    

    internal void AddNeighbour(MapNode node)
    {
        if (_neighbours.Contains(node))
            return;

        _neighbours.Add(node);
    }

    internal void RemoveNeighbour(MapNode node)
    {
        if (!_neighbours.Contains(node))
            return;

        _neighbours.Remove(node);
    }
}

internal class WaveEmitter : IWaveEmitter
{
    private readonly List<IEnemyCard> _enemyDeck;

    public WaveEmitter(IEnumerable<IEnemyCard> enemyDeck)
    {
        _enemyDeck = enemyDeck.ToList();
    }

    public IWave EmitWave(int size)
    {
        // TODO: Draw {size} cards from the enemy deck

        return new Wave(_enemyDeck.PickNUnique(size));
    }
}

internal class Wave : IWave
{
    private readonly List<IEnemyCard> _enemies;

    public Wave(IEnumerable<IEnemyCard> enemies)
    {
        _enemies = enemies.ToList();
    }

    public int Strength => _enemies.Sum(e => e.Strength);

    public IEnumerable<IEnemyCard> Enemies => _enemies;

}

internal class Enemy : IEnemyCard
{
    private readonly List<EnemyType> _types;

    public Enemy(string name, int strength, Size size, IEnumerable<EnemyType> types)
    {
        Name = name;
        Strength = strength;
        Size = size;
        _types = types.ToList();
    }

    public string Name { get; }
    public int Strength { get; internal set; }
    public Size Size { get; }

    public IEnumerable<EnemyType> Types => _types;
}

internal class Building : Card, IBuilding
{
    private readonly Subject<Unit> _onDestroy = new Subject<Unit>();
    private readonly List<BuildingType> _types;
    private readonly List<Keyword> _keywords;

    public Building(string name, int cost, int defense, int hitPoints, IEnumerable<BuildingType>? types=null, IEnumerable<Keyword>? keywords=null) : base(name)
    {
        _types = types?.ToList() ?? new List<BuildingType>();
        Cost = cost;
        HitPoints = MaxHitPoints = hitPoints;
        _keywords = keywords?.ToList() ?? new List<Keyword>();
        Defense = defense;
    }

    public bool IsStation => _types.Any(t => t == BuildingType.Station);
    public int Cost { get; }
    public int MaxHitPoints { get; }
    public int HitPoints { get; private set; }
    public int Defense { get; }
    public IEnumerable<Keyword> Keywords => _keywords;
    public IEnumerable<BuildingType> Types => _types;
    public IMapNode? Node { get; internal set; }
    public IObservable<Unit> OnDestroyed { get; }

    public void Damage()
    {
        HitPoints--;

        if (HitPoints <= 0)
        {
            Destroy();
        }
        
    }

    public void Destroy()
    {
        _onDestroy.OnNext(Unit.Default);
    }

}

internal abstract class Card : ICard
{
    protected Card(string name)
    {
        Name = name;
    }

    public string Name { get; }
}
