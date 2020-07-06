using Archetype;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArchetypeTests
{
    [TestClass]
    public class TriggerTests : ArchetypeTestBase
    {
        [TestMethod]
        public void Trigger_IsAttachedToTarget()
        {
            (Friend2 as ITriggerAttachee<Unit>)
                .AttachTrigger(new DamageTrigger(delegate { }));

            Assert.AreEqual(1, Friend2.Triggers.Count);
        }

        [TestMethod]
        public void Trigger_CanHaveTwoTriggersAttached()
        {
            (Friend2 as ITriggerAttachee<Unit>)
                .AttachTrigger(new HealTrigger(delegate { }));

            (Friend2 as ITriggerAttachee<Unit>)
                .AttachTrigger(new HealTrigger(delegate { }));


            Assert.AreEqual(2, Friend2.Triggers.Count);
        }

        [TestMethod]
        public void Trigger_IsTriggeredOncePerEvent()
        {
            var valueToModify = 0;

            (Friend2 as ITriggerAttachee<Unit>)
                .AttachTrigger(new DamageTrigger(delegate { valueToModify++; }));

            Friend2.Damage(1);
            Friend2.Damage(1);

            Assert.AreEqual(2, valueToModify);
        }
    }
}
