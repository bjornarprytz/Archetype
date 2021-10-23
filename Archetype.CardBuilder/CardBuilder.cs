using Archetype.Core;
using System;
using System.Linq.Expressions;
using System.Text;

namespace Archetype.CardBuilder
{
    public class CardBuilder : BaseBuilder<CardData>
    {
        private StringBuilder _rulesTextBuilder = new ();
        
        internal CardBuilder(CardData template)
        {
            template ??= new CardData()
            {
                Id = Guid.NewGuid()
            };

            Construction = template;
        }

        

        public CardBuilder Name(string name)
        {
            Construction.Name = name;

            return this;
        }

        public CardBuilder Rarity(CardRarity rarity)
        {
            Construction.Rarity = rarity;

            return this;
        }

        public CardBuilder Cost(int cost)
        {
            Construction.Cost = cost;

            return this;
        }

        public CardBuilder Color(CardColor color)
        {
            Construction.Color = color;

            return this;
        }

       

        public CardBuilder Text(string text)
        {
            _rulesTextBuilder = new StringBuilder(text);

            return this;
        }
        
        public CardBuilder AddTextLine(string text)
        {
            _rulesTextBuilder.AppendLine(text);

            return this;
        }

        public CardBuilder Art(string link)
        {
            Construction.ImageUri = link;

            return this;
        }

        public CardBuilder Effect<TTarget, TResult>(Expression<Func<TTarget, IGameState, TResult>> expression)
            where TTarget : IGamePiece
        {
            Construction.Effects.Add(new EffectData<TTarget, TResult>(expression));

            return this;
        }
        
        public CardBuilder Effect<TTarget, TResult>(
            Expression<Func<TTarget, IGameState, bool>> validationExpression,
            Expression<Func<TTarget, IGameState, TResult>> resolutionExpression
            )
            where TTarget : IGamePiece
        {
            Construction.Effects.Add(new EffectData<TTarget, TResult>(resolutionExpression, validationExpression));

            return this;
        }

        protected override void PreBuild()
        {
            Construction.RulesText = _rulesTextBuilder.ToString();
            
            Console.WriteLine($"Creating card {Construction.Name}");
        }
    }
}
