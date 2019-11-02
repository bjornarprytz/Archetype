using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public abstract class Zone
    {
        public delegate void CardIn(Card cardThatEnteredZone);
        public delegate void CardOut(Card cardThatLeftZone);
        public event CardIn OnCardEntered;
        public event CardOut OnCardExited;

        public Zone(Unit owner) { Owner = owner; }
        public Unit Owner { get; private set; }
        public virtual void Out(Card cardToMove) { OnCardExited?.Invoke(cardToMove); }
        public virtual void Into(Card cardToMove) { OnCardEntered?.Invoke(cardToMove); }
    }
}
