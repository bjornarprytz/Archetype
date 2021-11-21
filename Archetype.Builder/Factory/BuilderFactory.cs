using Archetype.Dto.MetaData;
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
        
        public static EffectBuilder<TTarget> EffectBuilder<TTarget>()
            where TTarget : IGameAtom
        {
            return new EffectBuilder<TTarget>();
        }
        
        public static EffectBuilder EffectBuilder()
        {
            return new EffectBuilder();
        }
        
        public static NodeBuilder NodeBuilder()
        {
            return new NodeBuilder();
        }

        public static MapBuilder MapBuilder()
        {
            return new MapBuilder();
        }

        public static UnitBuilder UnitBuilder()
        {
            return new UnitBuilder();
        }
    }
}
