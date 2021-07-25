namespace Archetype.Core
{
    public interface IEnemy
    {
        public int Health { get; set; }

        public IEffectResult Damage(int i);
    }
}


