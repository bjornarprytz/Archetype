using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Builder.Factory
{
    public class BuilderFactory
    {
        public static CardBuilder CardBuilder()
        {
            return new CardBuilder();
        }
        
        public static EffectBuilder<TTarget, TResult> EffectBuilder<TTarget, TResult>()
            where TTarget : IGameAtom
        {
            return new EffectBuilder<TTarget, TResult>();
        }
        
        public static EffectBuilder<TResult> EffectBuilder<TResult>()
        {
            return new EffectBuilder<TResult>();
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
