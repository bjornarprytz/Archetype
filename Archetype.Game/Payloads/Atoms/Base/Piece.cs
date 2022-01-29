using System;
using System.Reactive.Subjects;
using Archetype.Game.Factory;
using Archetype.Game.Payloads.Context;
using Archetype.View.Atoms;
using Archetype.View.Atoms.Zones;
using Archetype.View.Events;

namespace Archetype.Game.Payloads.Atoms.Base
{
    internal abstract class Piece<T> : Atom, IPiece<T>
        where T : IPiece
    {
        protected readonly Subject<IAtomMutation<T>> Mutation = new();

        protected Piece(string name, IGameAtom owner) : base(owner)
        {
            Name = name;
        }

        public string Name { get; }
        
        public override IObservable<IAtomMutation> OnMutation => Mutation;
        IObservable<IAtomMutation<T>> IPiece<T>.OnMutation => Mutation;

        public IZone<T> CurrentZone { get; private set; }

        public IResult<IPiece<T>, IZone<T>> MoveTo(IZone<T> zone)
        {
            if (zone == CurrentZone)
                return ResultFactory.Null<IPiece<T>, IZone<T>>(this);

            CurrentZone?.Remove(Self);
            CurrentZone = zone;
            CurrentZone?.Add(Self);

            Mutation.OnNext(new AtomMutation<T>(Self));
            
            return ResultFactory.Create(this, CurrentZone);
        }

        protected abstract T Self { get; }
        IZoneFront IPieceFront.CurrentZone => CurrentZone;
    }
}