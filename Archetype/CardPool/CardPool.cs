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
                      
                      // Tick 0: Deal 4 damage to All Enemies
                      (0, new DamageTemplate(4, new TargetAll<Unit>((s, t) => s.EnemyOf(t)), (s) => s.Enemies)),
                      
                      // Tick 0: Heal 3 damage from 4 Allies
                      (0, new HealTemplate(3, TargetAny<Unit>.Ally(4), (s) => s.ActiveUnits)),

                      // Tick 1: Deal 4 damage to one Enemy with more life than source
                      (1, new DamageTemplate(4, 
                              new TargetAny<Unit>(1, 
                                  (s, t) => t.EnemyOf(s) && s.ResourceDifference<Life>(t) < 0), (s) => s.Enemies)) 
                      )                      
            {

            }
        }
    }
}
