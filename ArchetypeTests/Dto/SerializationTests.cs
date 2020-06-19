using Archetype;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace ArchetypeTests
{
    [TestClass]
    public class SerializationTests : ArchetypeTestBase
    {
        [TestMethod]
        public void ImmediateValue_Is_Serialized()
        {
            var data1 = new CardData
            {
                Actions = new List<ActionParameterData>
                {
                    new DamageParameterData
                    {
                        Strength = new ImmediateValue<int>(1),
                    }
                }
            };


            var json = CardSerializer.SerializeCardData(data1);

            var data2 = CardSerializer.DeserializeCardJson(json);

            var damageValue = ((ImmediateValue<int>)((DamageParameterData)data2.Actions.First()).Strength).Value;

            Assert.AreEqual(1, damageValue);
        }
    }
}
