using Archetype.Builder.Builders;
using Archetype.Game.Payloads.MetaData;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Builder.Factory
{
    public class BuilderFactory
    {

        public static CardPoolBuilder CardPoolBuilder()
        {
            return new CardPoolBuilder();
        }
        
        public static SetBuilder SetBuilder(string name)
        {
            return new SetBuilder(name);
        }
        public static CardBuilder CardBuilder(CardMetaData template=default)
        {
            return new CardBuilder(template);
        }
        
        public static CardEffectBuilder<TTarget> EffectBuilder<TTarget>()
            where TTarget : IGameAtom
        {
            return new CardEffectBuilder<TTarget>();
        }
        
        public static CardEffectBuilder EffectBuilder()
        {
            return new CardEffectBuilder();
        }
        
        public static NodeBuilder NodeBuilder()
        {
            return new NodeBuilder();
        }

        public static MapBuilder MapBuilder()
        {
            return new MapBuilder();
        }

        public static CreatureBuilder CreatureBuilder(CreatureMetaData template=default)
        {
            return new CreatureBuilder(template);
        }
        
        public static StructureBuilder StructureBuilder(StructureMetaData template=default)
        {
            return new StructureBuilder(template);
        }
    }
}
