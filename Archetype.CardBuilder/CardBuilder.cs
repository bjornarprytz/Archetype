using Archetype.Core;
using System;
using System.Linq.Expressions;
using System.Text;
using Archetype.CardBuilder.Extensions;

namespace Archetype.CardBuilder
{
    public class CardBuilder : BaseBuilder<CardData>
    {
        private StringBuilder _rulesTextBuilder = new ();
        
        internal CardBuilder(CardData template=null) : base(() => template ?? new CardData{ Id = Guid.NewGuid() })
        {
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
        
        public CardBuilder EffectBuilder<TTarget, TResult>(Action<EffectBuilder<TTarget, TResult>> builderProvider)
            where  TTarget : IGamePiece
        {
            var cbc = BuilderFactory.EffectBuilder<TTarget, TResult>(); // Input template data here

            builderProvider(cbc);

            Construction.Effects.Add(cbc.Build());

            return this;
        }
        
        public CardBuilder Effect<TTarget, TResult>(
            Func<TTarget, IGameState, TResult> resolveEffect,
            Func<TTarget, IGameState, bool> validateEffect=null,
            Func<TTarget, IGameState, string> rulesText=null
            )
            where  TTarget : IGamePiece
        {
            return EffectBuilder<TTarget, TResult>(provider => 
                provider
                    .Validate(validateEffect ?? ((piece, state) => true) )
                    .Resolve(resolveEffect)
                    .Text(rulesText ?? ((piece, state) => string.Empty) )
                );
        }

        protected override void PreBuild()
        {
            foreach (var effect in Construction.Effects)
            {
                string textLine;

                try
                {
                    textLine = effect.CallTextMethod(null, null);
                }
                catch (NullReferenceException)
                {
                    textLine = "Text error: Additional state needed";
                }
                
                AddTextLine(textLine);
            }

            Construction.RulesText = _rulesTextBuilder.ToString();
            
            Console.WriteLine($"Creating card {Construction.Name}");
        }
    }
}
