using Archetype.Core;

namespace Archetype.CardBuilder
{
    public class BuilderFactory
    {

        public static CardBuilder CardBuilder(CardData template = null)
        {
            return new CardBuilder(template);
        }
        
        public static EffectBuilder<TTarget, TResult> EffectBuilder<TTarget, TResult>(EffectData<TTarget, TResult> template = null)
            where TTarget : IGamePiece
        {
            return new EffectBuilder<TTarget, TResult>(template);
        }
        
        public static EffectBuilder<TResult> EffectBuilder<TResult>(EffectData<TResult> template = null)
        {
            return new EffectBuilder<TResult>(template);
        }

        public static TemplateBuilder TemplateBuilder(CardData template = null)
        {
            return new TemplateBuilder(template);
        }
    }
}
