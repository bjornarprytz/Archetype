namespace Archetype
{
    public class Move : Card
    {
        // A move made by an enemy

        public Move(string name, Zone zone) 
            : base(name, zone)
        {

        }

        public override void Resolve()
        {
            base.Resolve();
        }
    }
}