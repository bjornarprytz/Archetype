
using Archetype;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArchetypeTests
{
    [TestClass]
    public class ActionHandlerTests : ArchetypeTestBase
    {
        [TestMethod]
        public void Modifier_ActionModifierIsAttached()
        {
            var modifier = new OffensiveActionModifier<Unit, DamageActionArgs>();

            (Friend1 as IModifierAttachee<Unit>).AttachModifier(modifier);

            Assert.AreEqual(1, Friend1.Modifiers.Count);
        }

        [TestMethod]
        public void Modifier_ActionModifierIsDetached()
        {
            var modifier = new OffensiveActionModifier<Unit, DamageActionArgs>();

            (Friend1 as IModifierAttachee<Unit>).AttachModifier(modifier);

            Assert.AreEqual(1, Friend1.Modifiers.Count);

            (Friend1 as IModifierAttachee<Unit>).DetachModifier(modifier);

            Assert.AreEqual(0, Friend1.Modifiers.Count);
        }

        [TestMethod]
        public void Modifier_OffensiveActionModifierModifiesDamageAction()
        {
            var damage = 0;
            var damageModifier = 1;

            (Friend2 as IModifierAttachee<Unit>).AttachModifier(
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

            (Enemy1 as IModifierAttachee<Unit>).AttachModifier(
                new DefensiveActionModifier<Unit, DamageActionArgs>(damageModifier));

            var previousHealth = Enemy1.Life;

            var damageAction = new DamageActionArgs(Friend2, Enemy1, () => damage);

            damageAction.Execute();

            Assert.AreEqual(previousHealth - (damage + damageModifier), Enemy1.Life);
        }

        [TestMethod]
        public void Modifier_DefensiveActionModifiersStack()
        {
            var damage = 0;
            var damageModifier = 1;

            (Enemy1 as IModifierAttachee<Unit>).AttachModifier(
                new DefensiveActionModifier<Unit, DamageActionArgs>(damageModifier));

            (Enemy1 as IModifierAttachee<Unit>).AttachModifier(
                new DefensiveActionModifier<Unit, DamageActionArgs>(damageModifier));
                
            var previousHealth = Enemy1.Life;

            var damageAction = new DamageActionArgs(Friend2, Enemy1, () => damage);

            damageAction.Execute();

            Assert.AreEqual(previousHealth - (damage + (damageModifier*2)), Enemy1.Life);
        }

        [TestMethod]
        public void Modifier_ModifiersApplyMultiplierLast()
        {
            var damage = 0;
            var damageModifier = 1;
            var damageMultiplier = 2f;

            (Enemy1 as IModifierAttachee<Unit>).AttachModifier(
                new DefensiveActionModifier<Unit, DamageActionArgs>(multiplier: damageMultiplier));
            
            (Enemy1 as IModifierAttachee<Unit>).AttachModifier(
                new DefensiveActionModifier<Unit, DamageActionArgs>(damageModifier));
            
            (Enemy1 as IModifierAttachee<Unit>).AttachModifier(
                new DefensiveActionModifier<Unit, DamageActionArgs>(damageModifier));

            var previousHealth = Enemy1.Life;

            var damageAction = new DamageActionArgs(Friend2, Enemy1, () => damage);

            damageAction.Execute();

            var expectedRemainingHealth =
                previousHealth 
                - ((damage 
                + (damageModifier * 2)) 
                * damageMultiplier);

            Assert.AreEqual(expectedRemainingHealth, Enemy1.Life);
        }


        [TestMethod]
        public void ActionResponse_ReactionHandlerIsAttached()
        {
            (Friend1 as IResponseAttachee<Unit>)
                .AttachResponse(new ActionReaction<Unit, DamageActionArgs>(delegate { }));

            Assert.AreEqual(1, Friend1.Responses.Count);
        }

        [TestMethod]
        public void ActionResponse_ReactionHandlerIsDetached()
        {
            var response = new ActionReaction<Unit, DamageActionArgs>(delegate { });

            (Friend1 as IResponseAttachee<Unit>)
                .AttachResponse(response);

            Assert.AreEqual(1, Friend1.Responses.Count);

            (Friend1 as IResponseAttachee<Unit>)
                .DetachResponse(response);

            Assert.AreEqual(0, Friend1.Responses.Count);
        }

        [TestMethod]
        public void ActionResponse_ReactionHandlerCanAccessActionArgs()
        {
            var valToModify = 0;
            var damage = 2;

            (Friend1 as IResponseAttachee<Unit>)
                .AttachResponse(new ActionReaction<Unit, DamageActionArgs>((s, e) => valToModify = e.Strength)); ;

            new DamageActionArgs(Enemy1, Friend1, () => damage)
                .Execute();

            Assert.AreEqual(damage, valToModify);
        }

        [TestMethod]
        public void ActionResponse_FollowUpHandlerCanAccessActionArgs()
        {
            var valToModify = 0;
            var damage = 2;

            (Enemy1 as IResponseAttachee<Unit>)
                .AttachResponse(new ActionFollowUp<Unit, DamageActionArgs>((s, e) => valToModify = e.Strength)); ;

            new DamageActionArgs(Enemy1, Friend1, () => damage)
                .Execute();

            Assert.AreEqual(damage, valToModify);
        }
    }
}
