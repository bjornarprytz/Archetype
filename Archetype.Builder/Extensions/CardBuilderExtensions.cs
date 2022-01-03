using Archetype.Builder.Builders;
using Archetype.View.Primitives;

namespace Archetype.Builder.Extensions
{
    public static class CardBuilderExtensions
    {
        public static ICardBuilder Red(this ICardBuilder builder)
        {
            return builder.Color(CardColor.Red); 
        }
        public static ICardBuilder White(this ICardBuilder builder)
        {
            return builder.Color(CardColor.White); 
        }
        public static ICardBuilder Black(this ICardBuilder builder)
        {
            return builder.Color(CardColor.Black);
        }
        public static ICardBuilder Green(this ICardBuilder builder)
        {
            return builder.Color(CardColor.Green); 
        }
        public static ICardBuilder Blue(this ICardBuilder builder)
        {
            return builder.Color(CardColor.Blue); 
        }
    }
}