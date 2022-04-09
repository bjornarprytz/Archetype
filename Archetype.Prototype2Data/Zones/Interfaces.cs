using System.Net.Mime;
using Archetype.Prototype2Data.Base;
using Archetype.Prototype2Data.Cards;

namespace Archetype.Prototype2Data.Zones;


internal interface IStationStack : IStationStackView, IZone<IBuildingView>
{
    IBuilding Pop();
    void AddBuildings(IEnumerable<IBuilding> buildings);
}

public interface IStationStackView : IZoneView<IBuildingView>
{
    IBuildingView TopStation { get; }
    IEnumerable<IBuildingView> Stations { get; }
}


internal interface IBuildingDeck : IBuildingDeckView, IDeck<IBuildingView>
{
    
}

public interface IBuildingDeckView : IDeckView<IBuildingView>
{
    
}

internal interface ICrewDeck : ICrewDeckView, IDeck<ICrewView>
{
    
}

public interface ICrewDeckView : IDeckView<ICrewView>
{
    
}

internal interface IEnemyDeck : IEnemyDeckView, IDeck<IEnemyView>
{
    
}

public interface IEnemyDeckView : IDeckView<IEnemyView>
{
    
}

internal interface IDeck<T> : IDeckView<T>, IZone<T> where T : IGameAtom
{
    T Draw();
    void AddCards(IEnumerable<T> cards);
    void Shuffle();
}

public interface IDeckView<out T> : IZoneView<T> where T : IGameAtom
{
    int Count { get; }
    IZoneView<T> DiscardPile { get; }
}

internal interface IMapNode : IMapNodeView, IZone<IGameAtom>
{
    void Connect(IMapNode other);
    void Disconnect(IMapNode other);

    new IEnumerable<IMapNode> Neighbours { get; }
}

public interface IMapNodeView : IZoneView<IGameAtom>, IGameAtom
{
    int Exposure { get; }
    IEnumerable<IMapNodeView> Neighbours { get; }
}

public interface IExileView : IZoneView<IGameAtom>
{
    
}

internal interface IZone<T> : IZoneView<T> where T : IGameAtom
{
    void Add(T newThing);
    void Remove(T thing);
}


public interface IZoneView<out T> where T : IGameAtom
{
    IEnumerable<T> Contents { get; }
}