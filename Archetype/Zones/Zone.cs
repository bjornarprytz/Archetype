using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public abstract class Zone<T>: IEnumerable<T>
        where T : GamePiece 
    {
        public delegate void PieceIn(T cardThatEnteredZone);
        public delegate void PieceOut(T cardThatLeftZone);
        public event PieceIn OnEntered;
        public event PieceOut OnExited;

        public virtual void Out(T pieceToMove) { OnExited?.Invoke(pieceToMove); }
        public virtual void Into(T pieceToMove) { OnEntered?.Invoke(pieceToMove); }

        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
