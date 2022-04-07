using System.Reactive;

namespace Archetype.Prototype2Data;

public interface IGameView
{
    IGameState GameState { get; }

    void PlayBuilding(IBuilding building, IMapNode mapNode, IEnumerable<ICrew> payment);
    void PlayCrewToTask(ICrew crew, IBuilding building);
    void PlayCrewToDefend(ICrew crew, IMapNode mapNode);
    
    void StartGame();
    void EndTurn();
}

public interface IGameState
{
    IPlayer Player { get; }
    IMap Map { get; }
}

public interface IWave
{
    int Strength { get; }
    IEnumerable<IEnemyCard> Enemies { get; }
}

public interface IPlayer
{
    int Resources { get; }
    
    int CardsInDeck { get; }
    IEnumerable<ICard> Hand { get; }
    
    IObservable<ICard> OnCardDrawn { get; }
    IObservable<ICard> OnCardRemoved { get; }
}

public interface IMap
{
    IEnumerable<IMapNode> Nodes { get; }
    IMapNode? Root { get; }
}

public interface IWaveEmitter
{
    IWave EmitWave(int size);
}

public interface IMapNode
{
    int Exposure { get; }
    
    IEnumerable<IMapNode> Neighbours { get; }
    IWave? Wave { get; }

    IEnumerable<ICrew> Defenders { get; }
    IBuilding? Building { get; }
}

public interface IEnemyCard
{
    string Name { get; }
    int Strength { get; }
    Size Size { get; }
    IEnumerable<EnemyType> Types { get; }
}

public interface IBuilding : ICard
{
    bool IsStation { get; }
    int Cost { get; }
    int MaxHitPoints { get; }
    int HitPoints { get; }
    int Defense { get; }
    IEnumerable<Keyword> Keywords { get; }
    IEnumerable<BuildingType> Types { get; }
    IMapNode? Node { get; }
    
    IObservable<Unit> OnDestroyed { get; }

    void Damage();
    void Destroy();
}

public interface ICrew : ICard
{
    int Resources { get; }
    int Defense { get; }
    // TODO: Add extra effects
}

public interface ICard
{
    string Name { get; }
}

public interface ITask
{
    string Name { get; }
    bool IsOngoing { get; }
    string Effect { get; } 
}

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

public enum BuildingType
{
    Economy,
    Stronghold,
    Station
}

public enum Keyword
{
    Draw,
    ClearCutting,
    Ranged,
    Repair,
    RaiseDead,
}