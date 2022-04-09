using System.Reactive;
using System.Reactive.Subjects;
using Archetype.Prototype2Data.Base;
using Archetype.Prototype2Data.Cards;
using Archetype.Prototype2Data.Zones;

namespace Archetype.Prototype2Data;

internal class Enemy : Card<EnemyData, IEnemyView>, IEnemy
{
    public Enemy(EnemyData data) : base(data)
    { }

    protected override IEnemyView This => this;
}

internal class Crew : Card<CrewData, ICrewView>, ICrew 
{
    public Crew(CrewData data) : base(data)
    { }
    protected override ICrewView This => this;
}

internal class Building : Card<BuildingData, IBuildingView>, IBuilding
{
    private readonly Subject<Unit> _onDestroy = new ();

    public Building(BuildingData state) : base(state)
    { }

    public IObservable<Unit> OnDestroy => _onDestroy;

    public void Damage()
    {
        State = State with { Morale = State.Morale -1 };
    }
    
    public void Destroy()
    {
        State = State with { Morale = 0 };
        
        _onDestroy.OnNext(Unit.Default);
    }

    protected override IBuildingView This => this;
}

internal abstract class Card<TState, TZone> : 
    GameAtom, 
    IState<TState>,
    IZoned<TZone>
    where TZone :  IGameAtom
{
    private readonly Subject<ZoneChange<TZone>> _onZoneChange = new ();
    
    private readonly Subject<TState> _onMutation = new ();
    private TState _state;

    protected Card(TState state)
    {
        _state = BaseState = state;
    }
    

    public TState BaseState { get; }

    public TState State
    {
        get => _state;
        protected set
        {
            _state = value;
            _onMutation.OnNext(_state);
        }
    }

    public IObservable<TState> OnMutation => _onMutation;
    public IZone<TZone>? CurrentZone { get; private set; }

    IZoneView<TZone>? IZonedView<TZone>.CurrentZone => CurrentZone;

    public IObservable<ZoneChange<TZone>> OnZoneChange => _onZoneChange;
    
    public void MoveTo(IZone<TZone>? newZone)
    {
        var zoneChange = new ZoneChange<TZone>(CurrentZone, newZone);

        CurrentZone?.Remove(This);
        CurrentZone = newZone;
        newZone?.Add(This);
        
        _onZoneChange.OnNext(zoneChange);
    }

    public void SetState(TState state)
    {
        State = state;
    }
    
    protected abstract TZone This { get; }
    
}