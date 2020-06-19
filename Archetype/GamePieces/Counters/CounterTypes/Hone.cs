
namespace Archetype
{
    public class Hone : Counter<Card>
    {
        public override void Combine(Counter other)
        {
            Charges += other.Charges;
        }

        public override void Start()
        {
            throw new System.NotImplementedException();
        }

        public override void Stop()
        {
            throw new System.NotImplementedException();
        }

        public override void Tick()
        {
            throw new System.NotImplementedException();
        }
    }
}
