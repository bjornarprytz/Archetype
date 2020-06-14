using System;

namespace Archetype
{
    public class CardTriggerArgs : TriggerArgs
    {
        public Card Card { get; set; }

        public CardTriggerArgs(Card card)
        {
            Card = card;
        }
    }
}
