using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public static class CardPool
    {
        public class KillerSwing : CardTemplate
        {
            
            public KillerSwing()
                : base("Killer Swing", 
                      new Payment[] { new Payment<Mana>(4) },
                      new KeyValuePair<int, EffectTemplate>(
                          0, new DamageTemplate(4, new ChooseTargets<Unit>(2, Faction.Enemy))),
                      new KeyValuePair<int, EffectTemplate>(
                          0, new HealTemplate(3, new ChooseTargets<Unit>(0, Faction.Any)))
                      )                      
            {

            }



        }
    }
}
