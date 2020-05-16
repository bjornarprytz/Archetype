using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    /*
     Represents the structure of a card. Instances of this class should be 
     singular designs (e.g. Lightning Bolt or Llanowar Elves). These instances
     should be factories for actual copies of those cards (ie. the cards that 
     you draw and play with).
     */
    public class CardTemplate<T> where T : Card, new()
    {
        public T CreateCard() => new T();
    }
}
