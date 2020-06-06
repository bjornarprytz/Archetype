
namespace Archetype
{
    public class DamageActionArgs : ActionInfo
    {
        public int Strength { get; set; }

        public DamageActionArgs(Unit source, Unit target, int damage) : base(source, target)
        {
            Strength = damage;
        }

        protected override void Resolve()
        {
            (Target as Unit).Damage(Strength);
        }
    }
}
