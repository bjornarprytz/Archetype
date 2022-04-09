using Archetype.Prototype2Data.Base;
using Archetype.Prototype2Data.Cards;

namespace Archetype.Prototype2Data.Zones;


internal class MapNode : GameAtom, IMapNode
{
    private readonly List<IMapNode> _neighbours = new();
    private readonly Dictionary<Guid, IGameAtom> _contents = new();

    private readonly int _baseExposure;

    public MapNode(int exposure)
    {
        Exposure = _baseExposure = exposure;
    }

    public int Exposure { get; }
    public IEnumerable<IMapNodeView> Neighbours => _neighbours;
    IEnumerable<IMapNode> IMapNode.Neighbours => _neighbours;
    public IEnumerable<IGameAtom> Contents => _contents.Values;

    public void Add(IGameAtom atom)
    {
        _contents.Add(atom.Id, atom);
    }

    public void Remove(IGameAtom atom)
    {
        _contents.Remove(atom.Id);
    }

    public void Connect(IMapNode other)
    {
        if (_neighbours.Contains(other))
            return;

        _neighbours.Add(other);
    }

    public void Disconnect(IMapNode other)
    {
        if (!_neighbours.Contains(other))
            return;

        _neighbours.Remove(other);
    }

    
}

internal class StationStack : Zone<IBuildingView>, IStationStack
{
    private readonly Stack<IBuilding> _buildingStack = new ();

    public IBuildingView TopStation => _buildingStack.Peek();
    public IEnumerable<IBuildingView> Stations => _buildingStack;
    
    public IBuilding Pop()
    {
        return _buildingStack.Pop();
    }

    public void AddBuildings(IEnumerable<IBuilding> buildings)
    {
        foreach (var building in buildings)
        {
            _buildingStack.Push(building);
        }
    }
}

internal class CrewDeck : Deck<ICrewView>, ICrewDeck
{
}

internal class BuildingDeck : Deck<IBuildingView>, IBuildingDeck
{
}

internal class EnemyDeck : Deck<IEnemyView>, IEnemyDeck
{
}

internal abstract class Deck<T> : IDeck<T> where T : IZonedView<T>, IGameAtom
{
    private readonly List<T> _contents = new();
    
    private readonly Stack<T> _cards = new();
    
    private readonly Zone<T> _discardPile = new();


    public IEnumerable<T> Contents => _contents;

    public IZoneView<T> DiscardPile => _discardPile;

    public int Count => _cards.Count;
    
    
    public T Draw()
    {
        return _cards.Pop();
    }

    public void AddCards(IEnumerable<T> cards)
    {
        foreach (var card in cards)
        {
            _cards.Push(card);
        }
    }

    public void Shuffle()
    {
        var cards = _cards.ToList();
        
        _cards.Clear();

        foreach (var card in cards.Shuffle())
        {
            _cards.Push(card);
        }
    }
    
    public void Add(T newThing)
    {
        _contents.Add(newThing);
    }

    public void Remove(T thing)
    {
        _contents.Remove(thing);
    }
}

internal class Zone<T> : IZone<T> where T : IGameAtom
{
    private readonly List<T> _contents = new();

    public IEnumerable<T> Contents => _contents;

    public void Add(T thing)
    {
        _contents.Add(thing);
    }

    public void Remove(T thing)
    {
        _contents.Remove(thing);
    }

    internal void Clear()
    {
        _contents.Clear();
    }
}