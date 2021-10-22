using System;
using System.Linq.Expressions;
using Archetype.Core;

namespace Archetype.CardBuilder.Extensions
{
    public static class BuilderExtensions
    {
        public static TBuilder Red<TBuilder>(this TBuilder builder)
            where TBuilder : CardBuilder
        {
            return builder.Color(CardColor.Red) as TBuilder; 
        }
        public static TBuilder White<TBuilder>(this TBuilder builder)
            where TBuilder : CardBuilder
        {
            return builder.Color(CardColor.White) as TBuilder; 
        }
        public static TBuilder Black<TBuilder>(this TBuilder builder)
            where TBuilder : CardBuilder
        {
            return builder.Color(CardColor.Black) as TBuilder; 
        }
        public static TBuilder Green<TBuilder>(this TBuilder builder)
            where TBuilder : CardBuilder
        {
            return builder.Color(CardColor.Green) as TBuilder; 
        }
        public static TBuilder Blue<TBuilder>(this TBuilder builder)
            where TBuilder : CardBuilder
        {
            return builder.Color(CardColor.Blue) as TBuilder; 
        }

        public static TBuilder Attack<TBuilder>(this TBuilder builder, int strength)
            where TBuilder : CardBuilder
        {
            return builder
                .AddTextLine($"Deal {strength}")
                .Effect<IEnemy, int>((target, state) => target.Attack(strength)) as TBuilder;
        }
        
        public static TBuilder Heal<TBuilder>(this TBuilder builder, int strength)
            where TBuilder : CardBuilder
        {
            return builder
                .AddTextLine($"Heal {strength}")
                .Effect<IEnemy, int>((target, state) => target.Heal(strength)) as TBuilder;
        }
    }
}