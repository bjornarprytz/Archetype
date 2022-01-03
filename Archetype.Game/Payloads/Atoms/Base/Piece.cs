using System;
using System.Reactive.Subjects;
using Archetype.Game.Factory;
using Archetype.Game.Payloads.Context;
using Archetype.View.Atoms;
using Archetype.View.Atoms.Zones;

namespace Archetype.Game.Payloads.Atoms.Base
{
    public abstract class Piece<T> : Atom, IZoned<T>
        where T : class, IGameAtom, IZoned<T>
    {
        private readonly Subject<ZoneTransition<T>> _onTransition = new();
        private IZone<T> _currentZone;
        protected Piece(IGameAtom owner) : base(owner) { }

        public IObservable<ZoneTransition<T>> Transition => _onTransition;

        public IZone<T> CurrentZone
        {
            get => _currentZone;
            private set 
            {
                if (value == _currentZone)
                    return;

                var prevZone = _currentZone;
                
                _currentZone = value;
                
                _onTransition.OnNext(new ZoneTransition<T>(prevZone, _currentZone, this));
            } 
        }

        public IResult<IZoned<T>, ZoneTransition<T>> MoveTo(IZone<T> zone)
        {
            if (zone == CurrentZone)
                return ResultFactory.Null<IZoned<T>, ZoneTransition<T>>(this);

            var prevZone = CurrentZone;
            
            CurrentZone = zone;
            
            zone._Place(Self);

            return ResultFactory.Create(this, new ZoneTransition<T>(prevZone, _currentZone, this));
        }

        protected abstract T Self { get; }
        IZoneFront IZonedFront.CurrentZone => CurrentZone;
    }
}