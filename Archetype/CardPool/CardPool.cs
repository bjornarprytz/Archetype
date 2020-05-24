namespace Archetype
{
    public static class CardPool
    {
        public class KillerSwing : Card
        {

            public int Damage = 4;

            public KillerSwing()
            {
                Name = "Killer Swing";
                Cost = 4;

            }

            public override TargetInfo GetRequirements(GameState gameState) => TargetInfo.Exactly(3, gameState.Battlefield.Enemies);

            protected override void PlayActual(PlayCardArgs args, IEffectQueue effectQueue)
            {
                var targets = args.Targets.CastTargets<Unit>();

                foreach(var target in targets)
                {
                    effectQueue.Enqueue(new Effect((_) => target.Damage(new DamageInfo(Owner, Damage))));
                }
            }
        }

        public class DummyCard : Card
        {

            public int Damage = 69;
            public DummyCard()
            {
                Name = "Dummy Card";
                Cost = 69;

            }

            public override TargetInfo GetRequirements(GameState gameState) => TargetInfo.Exactly(3, gameState.Battlefield.Enemies);

            protected override void PlayActual(PlayCardArgs args, IEffectQueue effectQueue)
            {
                var targets = args.Targets.CastTargets<Unit>();

                foreach (var target in targets)
                {
                    effectQueue.Enqueue(new Effect((_) => target.Damage(new DamageInfo(Owner, Damage))));
                }
            }
        }
    }
}
