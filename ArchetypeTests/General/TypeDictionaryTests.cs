using Archetype;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArchetypeTests
{
    [TestClass]
    public class TypeDictionaryTests
    {
        private TypeDictionary<object> TDict { get; set; }
        private TypeDictionary<ActionModifier<Unit>> ConstrainedTDict { get; set; }

        [TestInitialize]
        public void InitializeTests()
        {
            TDict = new TypeDictionary<object>();
            ConstrainedTDict = new TypeDictionary<ActionModifier<Unit>>();
        }

        [TestMethod]
        public void TypeDictionary_ReturnsObject()
        {
            TDict.Set<string>("hei");

            var str = TDict.Get<string>();

            Assert.AreEqual("hei", str);
        }

        [TestMethod]
        public void TypeDictionary_ObjectIsNotContained_ReturnsNull()
        {
            TDict.Set<int>("hei");

            var str = TDict.Get<string>();

            Assert.IsNull(str);
        }

        [TestMethod]
        public void TypeDictionary_OverwritesEntries()
        {
            TDict.Set<string>("hei");
            TDict.Set<string>("nei");

            var str = TDict.Get<string>();

            Assert.AreEqual("nei", str);
        }

        [TestMethod]
        public void TypeDictionary_RemovesEntry()
        {
            TDict.Set<string>("hei");

            TDict.Remove<string>();

            var str = TDict.Get<string>();

            Assert.IsNull(str);
        }

        [TestMethod]
        public void TypeDictionary_ConstraintAllowsSubclasses()
        {
            var mod = new DefensiveActionModifier<Unit, DamageActionArgs>();

            ConstrainedTDict.Set<DefensiveActionModifier<Unit, DamageActionArgs>>(mod);

            Assert.AreEqual(mod, ConstrainedTDict.Get<DefensiveActionModifier<Unit, DamageActionArgs>>());
        }

        [TestMethod]
        public void TypeDictionary_ConstraintAllowsMultipleSubclasses()
        {
            var mod1 = new DefensiveActionModifier<Unit, DamageActionArgs>();
            var mod2 = new DefensiveActionModifier<Unit, HealActionArgs>();
            
            ConstrainedTDict.Set<DefensiveActionModifier<Unit, DamageActionArgs>>(mod1);
            ConstrainedTDict.Set<DefensiveActionModifier<Unit, HealActionArgs>>(mod2);

            Assert.AreEqual(mod1, ConstrainedTDict.Get<DefensiveActionModifier<Unit, DamageActionArgs>>());
            Assert.AreEqual(mod2, ConstrainedTDict.Get<DefensiveActionModifier<Unit, HealActionArgs>>());
        }
    }
}
