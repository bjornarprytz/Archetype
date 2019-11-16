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
                      // Cost: 4 mana
                      new Payment[] { new Payment<Mana>(4) },
                      
                      // Tick 0: Deal 4 damage to 4 Enemies
                      new KeyValuePair<int, EffectTemplate>(
                          0, new DamageTemplate(4, TargetParams<Unit>.Enemy(4))),
                      
                      // Tick 0: Heal 3 damage from 4 Allies
                      new KeyValuePair<int, EffectTemplate>(
                          0, new HealTemplate(3, TargetParams<Unit>.Ally(4))),

                      // Tick 1: Deal 4 damage to one Enemy with more life than source
                      new KeyValuePair<int, EffectTemplate>(
                          1, new DamageTemplate(4, 
                              new TargetParams<Unit>(1, 
                                  (s, t) => t.EnemyOf(s) && t.Resources.Amount<Life>() > s.Resources.Amount<Life>()))) 
                      )                      
            {

            }
        }
    }
}
