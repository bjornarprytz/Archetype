
using Archetype;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArchetypeTests
{
    [TestClass]
    public class ModifierTests : ArchetypeTestBase
    {
        [TestMethod]
        public void Modifier_ActionModifierIsAttached()
        {
            var modifier = new OffensiveActionModifier<Unit, DamageActionArgs>();

            Friend1.AttachModifier(modifier);

            Assert.AreEqual(1, Friend1.ActionModifiers.Count);
        }

        [TestMethod]
        public void Modifier_ActionModifierIsDetached()
        {
            var modifier = new OffensiveActionModifier<Unit, DamageActionArgs>();

            Friend1.AttachModifier(modifier);

            Assert.AreEqual(1, Friend1.ActionModifiers.Count);

            Friend1.DetachModifier<OffensiveActionModifier<Unit, DamageActionArgs>>();

            Assert.AreEqual(0, Friend1.ActionModifiers.Count);
        }

        [TestMethod]
        public void Modifier_OffensiveActionModifierModifiesDamageAction()
        {
            var damage = 0;
            var damageModifier = 1;

            Friend2.AttachModifier(
                new OffensiveActionModifier<Unit, DamageActionArgs>(damageModifier));

            var previousHealth = Enemy1.Life;

            var damageAction = new DamageActionArgs(Friend2, Enemy1, () => damage);

            damageAction.Execute();

            Assert.AreEqual(previousHealth - (damage + damageModifier), Enemy1.Life);
        }

        [TestMethod]
        public void Modifier_DefensiveActionModifierModifiesDamageAction()
        {
            var damage = 0;
            var damageModifier = 1;

            Enemy1.AttachModifier(
                new DefensiveActionModifier<Unit, DamageActionArgs>(damageModifier));

            var previousHealth = Enemy1.Life;

            var damageAction = new DamageActionArgs(Friend2, Enemy1, () => damage);

            damageAction.Execute();

            Assert.AreEqual(previousHealth - (damage + damageModifier), Enemy1.Life);
        }
    }
}
