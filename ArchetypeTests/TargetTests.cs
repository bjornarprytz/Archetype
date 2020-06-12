using System;
using System.Collections.Generic;
using System.Linq;
using Archetype;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArchetypeTests
{
    [TestClass]
    public class TargetTests : ArchetypeTestBase
    {
        [TestInitialize]
        public override void InitializeTests()
        {
            base.InitializeTests();
        }

        [TestMethod]
        public void AttackCardIncludesAllEnemiesAsOptionalTargets()
        {
            var attackCard = AttackCard.MakeCopy(Friend1);

            var targetInfo = attackCard.GetTargetRequirements(GameState);

            var options = targetInfo.First().Options.ToList();

            Assert.AreEqual(3, options.Count);
            Assert.IsTrue(options.Contains(Enemy1));
            Assert.IsTrue(options.Contains(Enemy2));
            Assert.IsTrue(options.Contains(Enemy3));
        }

        [TestMethod]
        public void HealCardDoesNotRequireTargetsForFirstAbility()
        {
            var healCard = HealCard.MakeCopy(Friend1);

            var targetInfo = healCard.GetTargetRequirements(GameState).ToList();

            Assert.IsTrue(targetInfo.First().IsAutomatic);
            Assert.IsFalse(targetInfo.Last().IsAutomatic);

            var selfAutomaticSelection = targetInfo.First().ConfirmedSelection.ToList();
            var healOptions = targetInfo.Last().Options.ToList();


            Assert.AreEqual(1, selfAutomaticSelection.Count);
            Assert.AreEqual(2, healOptions.Count);

            Assert.IsTrue(healOptions.Contains(Friend1));
            Assert.IsTrue(healOptions.Contains(Friend2));
        }

        [TestMethod]
        public void HealCardAffectsSelf()
        {
            Friend2.Damage(5);

            var healCard = HealCard.MakeCopy(Friend1);
            var damageStrength = (healCard.Data.Actions.First() as DamageParameterData).Strength;
            var healStrength = (healCard.Data.Actions.Last() as HealParameterData).Strength;

            var targetInfo = healCard.GetTargetRequirements(GameState).ToList();

            targetInfo.Last().Add(Friend2);

            var friend1PrePlayLife = Friend1.Life;
            var friend2PrePlayLife = Friend2.Life;

            GameState.StartTurn(Friend1);
            GameState.TakeAction(new PlayCardAction(Friend1, healCard, new PlayCardArgs(targetInfo)));

            GameState.Update();

            Assert.AreEqual(friend1PrePlayLife - damageStrength, Friend1.Life);
            Assert.AreEqual(friend2PrePlayLife + healStrength, Friend2.Life);
        }
    }
}
