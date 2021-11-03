using Archetype.Game.Payloads.Pieces;

namespace Archetype.CardBuilder.Factory
{
    public class BuilderFactory
    {
        public static CardBuilder CardBuilder()
        {
            return new CardBuilder();
        }
        
        public static EffectBuilder<TTarget, TResult> EffectBuilder<TTarget, TResult>()
            where TTarget : IGamePiece
        {
            return new EffectBuilder<TTarget, TResult>();
        }
        
        public static EffectBuilder<TResult> EffectBuilder<TResult>()
        {
            return new EffectBuilder<TResult>();
        }
    }
}
