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
    public abstract class CardTemplate
    {
        public CompoundPayment Cost;
        public string Name { get; set; }
        public string RulesText { get; private set; }
        internal Dictionary<int, List<EffectTemplate>> EffectSpan { get; set; }


        internal CardTemplate(string name, Payment<Resource>[] cost, Dictionary<int, List<EffectTemplate>> effectSpan)
        {
            Name = name;
            Cost = new CompoundPayment(cost);
            EffectSpan = effectSpan;
        }

        public virtual Card CreateCard() { return new Card("Dummy", Cost); }
    }
}
