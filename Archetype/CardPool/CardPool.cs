

using System.Collections.Generic;

namespace Archetype
{
    public static class CardPool
    {
        public class KillerSwing : Card
        {

            public KillerSwing()
            {
                Name = "Killer Swing";
                Cost = 4;

            }

            protected override List<EffectTemplate> CreateEffectList()
            {
                List<EffectTemplate> effects = new List<EffectTemplate>();

                // Deal 4 damage to All Enemies
                effects.Add(new DamageTemplate(4, new TargetAll<Unit>((s, t) => s.EnemyOf(t)), (g) => g.Battlefield));
                // Heal 3 damage from 4 Allies
                effects.Add(new HealTemplate(3, TargetAny<Unit>.Ally(4), (g) => g.Battlefield));
                // Deal 4 damage to one Enemy with more life than source
                effects.Add(new DamageTemplate(4,
                              new TargetAny<Unit>(1,
                                  (s, t) => t.EnemyOf(s) && s.Life < t.Life), (g) => g.Battlefield));

                return effects;
            }
        }

        public class DummyCard : Card
        {

            public DummyCard()
            {
                Name = "Dummy Card";
                Cost = 69;

            }

            protected override List<EffectTemplate> CreateEffectList()
            {
                List<EffectTemplate> effects = new List<EffectTemplate>();

                // Deal 1 damage to self
                effects.Add(new DamageTemplate(1, new TargetSelf(), (g) => g.Battlefield));

                return effects;
            }
        }
    }
}
