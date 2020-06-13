
namespace Archetype
{
    public class UnitTriggerData : TriggerData
    {
        public UnitEvent When { get; set; }

        // Condition (
        
        // Resolution (EventHandler)

        public override ITrigger CreateTrigger()
        {
            throw new System.NotImplementedException();
        }
    }
}
