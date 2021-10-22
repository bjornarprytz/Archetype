using Archetype.Core;

namespace Archetype.CardBuilder.Extensions
{
    public static class BuilderExtensions
    {
        public static TBuilder Red<TBuilder>(this TBuilder builder) where  TBuilder : CardBuilder<CardData, TBuilder> => builder.Color(CardColor.Red);
        public static TBuilder White<TBuilder>(this TBuilder builder) where  TBuilder : CardBuilder<CardData, TBuilder> => builder.Color(CardColor.White);
        public static TBuilder Black<TBuilder>(this TBuilder builder) where  TBuilder : CardBuilder<CardData, TBuilder> => builder.Color(CardColor.Black);
        public static TBuilder Green<TBuilder>(this TBuilder builder) where  TBuilder : CardBuilder<CardData, TBuilder> => builder.Color(CardColor.Green);
        public static TBuilder Blue<TBuilder>(this TBuilder builder) where  TBuilder : CardBuilder<CardData, TBuilder> => builder.Color(CardColor.Blue); }
}