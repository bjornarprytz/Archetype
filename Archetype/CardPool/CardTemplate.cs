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
    public class CardTemplate
    {
        public string Name { get; set; }
        public string RulesText { get; private set; }
        public EffectSpan EffectSpan { get; set; }


        public CardTemplate(string name, EffectSpan effectSpan)
        {
            Name = name;
            EffectSpan = effectSpan;
            RulesText = effectSpan.GenerateRulesText();
        }

        public static CardTemplate Dummy(string name)
        {
            return new CardTemplate(name, new EffectSpan(new Dictionary<int, List<Effect>>()));
        }
    }
}
