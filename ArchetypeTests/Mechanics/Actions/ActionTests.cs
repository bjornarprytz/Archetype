using Archetype;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArchetypeTests
{
    [TestClass]
    public class ActionTests : ArchetypeTestBase
    {
        [TestMethod]
        public void DamageAffectsTarget()
        {
            var damageValue = 3;

            var damage = new DamageActionArgs(Friend1, Enemy1, () => damageValue);

            var expectedHealth = Enemy1.Life - damageValue;

            damage.Execute();

            Assert.AreEqual(expectedHealth, Enemy1.Life);
        }

        [TestMethod]
        public void HealAffectsTarget()
        {
            var healValue = 3;

            Friend2.Damage(4);

            var heal = new HealActionArgs(Friend1, Friend2, () => healValue);

            var expectedHealth = Friend2.Life + healValue;

            heal.Execute();

            Assert.AreEqual(expectedHealth, Friend2.Life);
        }

        [TestMethod]
        public void MillAffectsTarget()
        {
            InsertCardsIntoZone(5, Enemy1, Enemy1.Deck);

            var millValue = 3;

            var mill = new MillActionArgs(Friend1, Enemy1, () => millValue);

            var expectedCardsLeft = Enemy1.Deck.Count - millValue;

            mill.Execute();

            Assert.AreEqual(expectedCardsLeft, Enemy1.Deck.Count);
        }
    }
}
