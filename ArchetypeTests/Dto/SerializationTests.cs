using Archetype;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace ArchetypeTests
{
    [TestClass]
    public class SerializationTests : ArchetypeTestBase
    {
        private CardData cardData;

        [TestInitialize]
        public override void InitializeTests()
        {
            cardData = new CardData
            {
                Cost = 1,
                Actions = new List<ActionParameterData>
                {
                    new UnitTriggerData
                    {
                        Cause = UnitTriggerCause.Damaged,
                        TriggerAction = new HealParameterData
                        {
                            Strength = new Immediate<int>(3),
                            TargetRequirements = new TargetRequirementData
                            {
                                Predicate = new UnitPredicateData(),
                                Selection = new AllSelectionData(),
                            }
                        }
                    }
                }
            };


            base.InitializeTests();
        }

        [TestMethod]
        public void CardData_SerializationWorksBothWays()
        {
            var json = CardSerializer.SerializeCardData(cardData);

            var cardData2 = CardSerializer.DeserializeCardJson(json);

            Assert.AreEqual(cardData.GetHashCode(), cardData2.GetHashCode());
        }

        [TestMethod]
        public void CardData_CreatesCorrectCard()
        {
            var card = cardData.MakeCopy(Friend1);

            card.Play(
                new PlayCardArgs(
                    new List<ISelectionInfo<ITarget>>
                    {
                        new AllSelectionInfo<ITarget>(
                            new List<ITarget> { Enemy1 })

                    }),
                    GameState);

            GameState.ActionQueue.ResolveAll();

            Assert.AreEqual(1, Enemy1.Triggers.Count);
        }
    }
}
