using System;
using System.ComponentModel;
using System.Linq.Expressions;
using Archetype.Core;
using Archetype.Game.Payloads.Pieces;

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

        public static TBuilder Attack<TBuilder>(this TBuilder builder, int strength, int targetIndex=0)
            where TBuilder : CardBuilder
        {
            return builder
                .EffectBuilder<IUnit, int>(provider => 
                    provider
                        .TargetIndex(targetIndex)
                        .Resolve((target, state) => target.Attack(strength))
                        .Text($"Deal {strength}")
                    ) as TBuilder;
        }
        
        public static TBuilder Heal<TBuilder>(this TBuilder builder, int strength, int targetIndex=0)
            where TBuilder : CardBuilder
        {
            return builder
                .EffectBuilder<IUnit, int>(provider => 
                    provider
                        .TargetIndex(targetIndex)
                        .Resolve((target, state) => target.Heal(strength))
                        .Text($"Heal {strength}")
                ) as TBuilder;
        }
    }
}