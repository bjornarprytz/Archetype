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
        public int Cost;
        public string Name { get; set; }
        public string RulesText { get; private set; }
        internal Dictionary<int, List<EffectTemplate>> EffectSpan { get; set; }


        internal CardTemplate(string name, int cost, params (int, EffectTemplate)[] effects)
        {
            Name = name;
            Cost = cost;
            EffectSpan = new Dictionary<int, List<EffectTemplate>>();
            foreach(var effect in effects)
            {
                if (EffectSpan.ContainsKey(effect.Item1)) EffectSpan.Add(effect.Item1, new List<EffectTemplate>());

                EffectSpan[effect.Item1].Add(effect.Item2);
            }
        }

        public virtual Card CreateCard() { return new Card("Dummy", Cost); }
    }
}
