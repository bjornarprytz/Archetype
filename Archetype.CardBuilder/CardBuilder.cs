using Archetype.Core;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Archetype.CardBuilder
{
    public abstract class CardBuilder<T, TBuilder> : BaseBuilder<T>
        where T : CardData, new()
        where TBuilder : CardBuilder<T, TBuilder>
    {
        protected CardBuilder(T template)
        {
            if (template == null) template = new T();

            Construction = template; // TODO: set new Guid
        }

        public Action<TBuilder> ToProvider()
        {
            return provider => provider.Construction = Construction;
        }

        public TBuilder Name(string name)
        {
            Construction.Name = name;

            return this as TBuilder;
        }

        public TBuilder Rarity(CardRarity rarity)
        {
            Construction.Rarity = rarity;

            return this as TBuilder;
        }

        public TBuilder Cost(int cost)
        {
            Construction.Cost = cost;

            return this as TBuilder;
        }

        public TBuilder Color(CardColor color)
        {
            Construction.Color = color;

            return this as TBuilder;
        }

       

        public TBuilder Text(string text)
        {
            Construction.RulesText = text;

            return this as TBuilder;
        }

        public TBuilder Art(string link)
        {
            Construction.ImageUri = link;

            return this as TBuilder;
        }

        public TBuilder Effect<TTarget, TResult>(Expression<Func<TTarget, IGameState, TResult>> expression)
            where TTarget : IGamePiece
        {
            Construction.Effects.Add(new CardEffect<TTarget, TResult>(expression));

            return this as TBuilder;
        }
        
        public TBuilder Effect<TTarget, TResult>(
            Expression<Func<TTarget, IGameState, bool>> validationExpression,
            Expression<Func<TTarget, IGameState, TResult>> resolutionExpression
            )
            where TTarget : IGamePiece
        {
            Construction.Effects.Add(new CardEffect<TTarget, TResult>(resolutionExpression, validationExpression));

            return this as TBuilder;
        }

        protected override void PreBuild()
        {
            Console.WriteLine($"Creating card {Construction.Name}");
        }
    }
}
