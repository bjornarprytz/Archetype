
namespace Archetype
{
    public abstract class ActionInfo
    {
        public Unit Source { get; protected set; }
        public int Strength { get; set; }

        public ActionInfo(Unit source, int strength)
        {
            Source = source;
            Strength = strength;
        }
    }
}
