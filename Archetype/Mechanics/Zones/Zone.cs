using System;
using System.Collections;
using System.Collections.Generic;
namespace Archetype
{
    public abstract class Zone<T>: IEnumerable<T>
        where T : IZoned<T> 
    {
        public event Action<T> OnEntered;
        public event Action<T> OnExited;

        public void Eject(T pieceToMove) 
        {
            EjectInternal(pieceToMove);
            OnExited?.Invoke(pieceToMove);
        }
        public void Insert(T pieceToMove) 
        {
            InsertInternal(pieceToMove);
            OnEntered?.Invoke(pieceToMove); 
        }

        protected abstract void InsertInternal(T pieceToMove);
        protected abstract void EjectInternal(T pieceToEject);

        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
