
namespace Archetype
{
    public class Poison : Counter<Unit>
    {
        public Poison(int strength)
        {
            Charges = strength;
        }

        public override void Combine(Counter other)
        {
            Charges += other.Charges;
        }

        public override void Start()
        {
            
        }

        public override void Stop()
        {
            
        }

        public override void Tick()
        {
            // TODO: Make a way to differentiate damage from attacks and poison (allow simpler formulation)

            // Owner.Damage(new DamageEffect(Charges, new EffectArgs()))
        }
    }
}
