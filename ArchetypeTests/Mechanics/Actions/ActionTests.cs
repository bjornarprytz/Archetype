using Archetype;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace ArchetypeTests
{
    [TestClass]
    public class ActionTests : ArchetypeTestBase
    {
        [TestMethod]
        public void DamageAffectsTarget()
        {
            var damageValue = 3;
            var expectedHealth = Enemy1.Life - damageValue;

            var damage = new DamageActionArgs(Friend1, Enemy1, () => damageValue);

            damage.Execute();

            Assert.AreEqual(expectedHealth, Enemy1.Life);
        }

        [TestMethod]
        public void HealAffectsTarget()
        {
            var healValue = 3;

            Friend2.Damage(4);
            var expectedHealth = Friend2.Life + healValue;

            var heal = new HealActionArgs(Friend1, Friend2, () => healValue);

            heal.Execute();

            Assert.AreEqual(expectedHealth, Friend2.Life);
        }

        [TestMethod]
        public void MillAffectsTarget()
        {
            InsertCardsIntoZone(5, Enemy1, Enemy1.Deck);

            var millValue = 3;
            var expectedCardsLeft = Enemy1.Deck.Count - millValue;

            var mill = new MillActionArgs(Friend1, Enemy1, () => millValue);

            mill.Execute();

            Assert.AreEqual(expectedCardsLeft, Enemy1.Deck.Count);
        }

        [TestMethod]
        public void DrawAffectsTarget()
        {
            InsertCardsIntoZone(4, Friend1, Friend1.Deck);

            var drawValue = 3;
            var expectedCardsInDeck = Friend1.Deck.Count - drawValue;

            var draw = new DrawActionArgs(Friend1, Friend1, () => drawValue);

            draw.Execute();

            Assert.AreEqual(expectedCardsInDeck, Friend1.Deck.Count);
            Assert.AreEqual(drawValue, Friend1.Hand.Count);
        }

        [TestMethod]
        public void DiscardChoosesTargetsAutomaticallyIfValueIsHigherThanHandCount()
        {
            var discardValue = 2;

            InsertCardsIntoZone(discardValue, Enemy1, Enemy1.Hand);

            var discard = new DiscardActionArgs(Friend1, Enemy1, () => discardValue);

            discard.Execute();

            Assert.AreEqual(0, Enemy1.Hand.Count);
        }

        [TestMethod]
        public void DiscardTargetsCanChoose()
        {
            var discardValue = 1;

            InsertCardsIntoZone(discardValue + 1, Enemy1, Enemy1.Hand);

            var cardToDiscard = Enemy1.Hand.First();
            var cardToKeep = Enemy1.Hand.Last();

            ChoicesToMake.Add(cardToDiscard);

            var discard = new DiscardActionArgs(Friend1, Enemy1, () => discardValue);

            discard.Execute();

            Assert.IsTrue(Enemy1.Hand.Contains(cardToKeep));
            Assert.IsFalse(Enemy1.Hand.Contains(cardToDiscard));
        }

        [TestMethod]
        public void DiscardedCardsAreMovedToDiscardPile()
        {
            var discardValue = 2;

            InsertCardsIntoZone(discardValue, Enemy1, Enemy1.Hand);

            var discard = new DiscardActionArgs(Friend1, Enemy1, () => discardValue);

            discard.Execute();

            Assert.AreEqual(0, Enemy1.Hand.Count);
            Assert.AreEqual(discardValue, Enemy1.DiscardPile.Count);
        }
    }
}
