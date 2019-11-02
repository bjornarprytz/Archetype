using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Archetype;

namespace GameUnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Adventurer hero1 = new Adventurer(
                "Hercules",
                new List<Card>()
                {
                    Card.Dummy("High"),
                    Card.Dummy("Top"),
                    Card.Dummy("Low"),
                    Card.Dummy("Bottom"),
                });


            Adventurer hero2 = new Adventurer(
                "Hades",
                new List<Card>()
                {
                    Card.Dummy("High"),
                    Card.Dummy("Top"),
                });

            hero2.Deck.PutCardsOnBottom(new List<Card>()
            {
                Card.Dummy("Low"),
                Card.Dummy("Bottom"),
            });

            Assert.AreEqual(hero1.Deck.Cards.Count, hero2.Deck.Cards.Count);

            while (hero1.Deck.Cards.Count != 0)
            {
                Card card1 = hero1.Deck.PeekTop();
                Card card2 = hero2.Deck.PeekTop();


                Assert.AreEqual(card1.Name, card2.Name);
            }
        }
    }
}
