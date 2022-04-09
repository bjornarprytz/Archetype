using System.Reactive;
using Archetype.Prototype2Data.Base;
using Archetype.Prototype2Data.Zones;

namespace Archetype.Prototype2Data.Cards;

public interface IEnemyView : 
    IGameAtom, 
    IZonedView<IEnemyView>,
    IStateView<EnemyData>
{
}

internal interface IEnemy : 
    IEnemyView, 
    IZoned<IEnemyView>,
    IState<EnemyData>
{
    
}

public interface IBuildingView : 
    IGameAtom, 
    IZonedView<IBuildingView>,
    IStateView<BuildingData>
{
    IObservable<Unit> OnDestroy { get; }
}

internal interface IBuilding : 
    IBuildingView, 
    IZoned<IBuildingView>,
    IState<BuildingData>
{
    void Damage();
    void Destroy();
}

public interface ICrewView : 
    IGameAtom, 
    IZonedView<ICrewView>, 
    IStateView<CrewData>
{
    
}

internal interface ICrew : 
    ICrewView, 
    IZoned<ICrewView>,
    IState<CrewData>
{
    
}

public interface IStateView<out T>
{
    T BaseState { get; }
    T State { get; }
    
    IObservable<T> OnMutation { get; }
}

internal interface IState<T> : IStateView<T>
{
    void SetState(T state);
}

public interface IZonedView<T> where T : IGameAtom
{
    IZoneView<T>? CurrentZone { get; }
    IObservable<ZoneChange<T>> OnZoneChange { get; }
}
internal interface IZoned<T> : IZonedView<T> where T : IGameAtom
{
    new IZone<T>? CurrentZone { get; }
    void MoveTo(IZone<T>? newZone);
}

