
namespace Archetype
{
    public class DamageActionArgs : ActionInfo
    {

        public DamageActionArgs(Unit source, Unit target, int damage) : base(source, target, damage)
        {
        }

        protected override void Resolve()
        {
            Target.Damage(Strength);
        }
    }
}
