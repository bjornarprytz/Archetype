using System.Linq;
using Archetype;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArchetypeTests
{
    [TestClass]
    public class TargetTests : ArchetypeTestBase
    {
        [TestMethod]
        public void AllEnemiesAreOptionsForTargeting()
        {
            var selectionInfo = AllEnemies().GetSelectionInfo(Friend1, GameState);

            var options = selectionInfo.Options;

            Assert.AreEqual(3, options.Count);
            Assert.IsTrue(options.Contains(Enemy1));
            Assert.IsTrue(options.Contains(Enemy2));
            Assert.IsTrue(options.Contains(Enemy3));
        }

        [TestMethod]
        public void AllEnemiesAreAutoSelectedForTargeting()
        {
            var selectionInfo = AllEnemies().GetSelectionInfo(Friend1, GameState);

            var confirmed = selectionInfo.ConfirmedSelection;

            Assert.AreEqual(3, confirmed.Count);
            Assert.IsTrue(confirmed.Contains(Enemy1));
            Assert.IsTrue(confirmed.Contains(Enemy2));
            Assert.IsTrue(confirmed.Contains(Enemy3));
        }

        [TestMethod]
        public void AnyCardInAnEnemyHandAreNotAutoSelectedForTargeting()
        {
            SetupCardsInHands();

            var selectionInfo = AnyCardInAnEnemyHand().GetSelectionInfo(Friend1, GameState);

            var confirmed = selectionInfo.ConfirmedSelection;

            Assert.AreEqual(0, confirmed.Count);
        }

        [TestMethod]
        public void AnyCardInAnEnemyHandCannotSelectInvalidTarget()
        {
            SetupCardsInHands();

            var selectionInfo = AnyCardInAnEnemyHand().GetSelectionInfo(Friend1, GameState);


            Assert.IsFalse(selectionInfo.Add(Friend1.Hand.First()));
            Assert.AreEqual(0, selectionInfo.ConfirmedSelection.Count);
        }

        [TestMethod]
        public void AnyCardInAnEnemyHandCanSelectValidTarget()
        {
            SetupCardsInHands();

            var selectionInfo = AnyCardInAnEnemyHand().GetSelectionInfo(Friend1, GameState);

            Assert.IsTrue(selectionInfo.Add(Enemy1.Hand.First()));
            Assert.AreEqual(1, selectionInfo.ConfirmedSelection.Count);
        }

        [TestMethod]
        public void SelfTargetingIsAutoSelected()
        {
            var selfTargeting = Self().GetSelectionInfo(Friend1, GameState);

            Assert.IsTrue(selfTargeting.ConfirmedSelection.Contains(Friend1));
        }

        private void SetupCardsInHands()
        {
            AttackCard.MakeCopy(Enemy1).MoveTo(Enemy1.Hand);
            AttackCard.MakeCopy(Enemy1).MoveTo(Enemy1.Hand);

            CopyCard.MakeCopy(Enemy2).MoveTo(Enemy2.Hand);


            AttackCard.MakeCopy(Friend1).MoveTo(Friend1.Hand);

            HealCard.MakeCopy(Friend2).MoveTo(Friend2.Hand);
        }
    }
}
