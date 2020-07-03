
using Archetype;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArchetypeTests
{
    [TestClass]
    public class AttachmentTests : ArchetypeTestBase
    {
        [TestMethod]
        public void Trigger_IsAttachedToTarget()
        {
            var attachTrigger = new AttachTriggerActionArgs<Unit>(
                Friend1, Friend2, 
                new DamageTrigger(delegate { }));

            attachTrigger.Execute();

            Assert.AreEqual(1, Friend2.Triggers.Count);
        }

        [TestMethod]
        public void Trigger_CanHaveTwoTriggersAttached()
        {
            var attachTrigger = new AttachTriggerActionArgs<Unit>(
                Friend1, Friend2,
                new DamageTrigger(delegate { }));

            var attachTrigger2 = new AttachTriggerActionArgs<Unit>(
                Friend1, Friend2,
                new DamageTrigger(delegate { }));

            attachTrigger.Execute();
            attachTrigger2.Execute();

            Assert.AreEqual(2, Friend2.Triggers.Count);
        }

        [TestMethod]
        public void Trigger_IsTriggeredOncePerEvent()
        {
            var valueToModify = 0;

            var attachTrigger = new AttachTriggerActionArgs<Unit>(
                Friend1, Friend2,
                new DamageTrigger((s, e) => valueToModify++));

            attachTrigger.Execute();

            Friend2.Damage(1);
            Friend2.Damage(1);

            Assert.AreEqual(2, valueToModify);
        }
    }
}
