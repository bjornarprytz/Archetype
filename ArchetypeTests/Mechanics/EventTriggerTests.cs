﻿using Archetype;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace ArchetypeTests
{
    [TestClass]
    public class EventTriggerTests : ArchetypeTestBase
    {

        [TestMethod]
        public void OnDamagedIsTriggered()
        {
            int i = 3;

            Friend1.OnDamaged += delegate { i = 10; };

            Friend1.Damage(1);

            Assert.AreEqual(10, i);
        }

        [TestMethod]
        public void OnHealedIsTriggered()
        {
            int i = 3;

            Friend1.OnHealed += delegate { i = 10; };

            Friend1.Heal(1);

            Assert.AreEqual(10, i);
        }

        [TestMethod]
        public void AttachedTriggerIsTriggered()
        {
            var triggerCard = TriggerCard.MakeCopy(Friend1);

            var targetInfo = triggerCard.GetTargetRequirements(GameState);

            GameState.StartTurn(Friend1);
            GameState.TakeAction(new PlayCardAction(Friend1, triggerCard, new PlayCardArgs(targetInfo)));

            GameState.Update();

            Friend1.ShuffleDeck();
            GameState.Update();
            Assert.AreEqual(14, Friend1.Life);
            Friend1.ShuffleDeck();
            GameState.Update();
            Assert.AreEqual(13, Friend1.Life);
            
            // TODO : test different targets for 'Action' as well as 'TriggerAction'
        }
    }
}