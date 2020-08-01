using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace ArchetypeTests
{
    [TestClass]
    public class ZoneTests : ArchetypeTestBase
    {
        [TestMethod]
        public void CardIsMovedIntoHand()
        {
            var physicalCard = AttackCard.MakeCopy(Friend1);

            physicalCard.MoveTo(Friend1.Hand);

            Assert.IsTrue(Friend1.Hand.Contains(physicalCard));
        }

        [TestMethod]
        public void CardIsMovedChangesZone()
        {
            var physicalCard = AttackCard.MakeCopy(Friend1);
            physicalCard.MoveTo(Friend1.Hand);
            physicalCard.MoveTo(Friend1.DiscardPile);

            Assert.IsFalse(Friend1.Hand.Contains(physicalCard));
            Assert.IsTrue(Friend1.DiscardPile.Contains(physicalCard));
        }

        [TestMethod]
        public void UnitIsMovedToGraveyard()
        {
            Enemy1.MoveTo(GameState.Graveyard);

            Assert.IsTrue(GameState.Graveyard.Contains(Enemy1));
        }

        [TestMethod]
        public void UnitIsMovedToBattlefieldWhenItIsAlreadyThere()
        {
            Enemy1.MoveTo(GameState.Battlefield);

            Assert.IsTrue(GameState.Battlefield.Contains(Enemy1));
        }

        [TestMethod]
        public void UnitIsMovedToGraveyardAndBack()
        {
            Enemy1.MoveTo(GameState.Graveyard);
            Enemy1.MoveTo(GameState.Battlefield);

            Assert.IsTrue(GameState.Battlefield.Contains(Enemy1));
            Assert.IsFalse(GameState.Graveyard.Contains(Enemy1));
        }

        
    }
}
