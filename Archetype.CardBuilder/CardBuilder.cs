using Archetype.Core;
using System;
using System.Text;
using Archetype.Game.Payloads;
using Archetype.Game.Payloads.Metadata;
using Archetype.Game.Payloads.Pieces;

namespace Archetype.CardBuilder
{
    public class CardBuilder : IBuilder<ICard>
    {
        private StringBuilder _rulesTextBuilder = new ();
        private CardData _cardData;

        private Card _card;
        
        internal CardBuilder()
        {
            _cardData = new CardData();
            _card = new Card(_cardData);
        }

        public CardBuilder Name(string name)
        {
            _cardData.Name = name;

            return this;
        }

        public CardBuilder Rarity(CardRarity rarity)
        {
            _cardData.Rarity = rarity;

            return this;
        }

        public CardBuilder Cost(int cost)
        {
            _cardData.Cost = cost;

            return this;
        }

        public CardBuilder Color(CardColor color)
        {
            _cardData.Color = color;

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
            _cardData.ImageUri = link;

            return this;
        }

        public CardBuilder Target<TTarget>(Func<TTarget, IGameState, bool> validateTarget=null)
            where TTarget : IGamePiece
        {
            _card.Targets.Add(new Target<TTarget>(validateTarget));

            return this;
        }
        public CardBuilder Targets<TT1, TT2>(
            Func<TT1, IGameState, bool> validateTarget1=null, 
            Func<TT2, IGameState, bool> validateTarget2=null
        )
            where TT1 : IGamePiece
            where TT2 : IGamePiece
        {
            
            _card.Targets.Add(new Target<TT1>(validateTarget1));
            _card.Targets.Add(new Target<TT2>(validateTarget2));

            return this;
        }
        public CardBuilder Targets<TT1, TT2, TT3>(
            Func<TT1, IGameState, bool> validateTarget1=null, 
            Func<TT2, IGameState, bool> validateTarget2=null, 
            Func<TT3, IGameState, bool> validateTarget3=null 
            )
            where TT1 : IGamePiece
            where TT2 : IGamePiece
            where TT3 : IGamePiece
        {
            
            _card.Targets.Add(new Target<TT1>(validateTarget1));
            _card.Targets.Add(new Target<TT2>(validateTarget2));
            _card.Targets.Add(new Target<TT3>(validateTarget3));

            return this;
        }
        
        public CardBuilder EffectBuilder<TTarget, TResult>(Action<EffectBuilder<TTarget, TResult>> builderProvider)
            where  TTarget : IGamePiece
        {
            var cbc = BuilderFactory.EffectBuilder<TTarget, TResult>(); // Input template data here

            builderProvider(cbc);
            
            _card.Effects.Add(cbc.Build());

            return this;
        }
        
        public CardBuilder EffectBuilder<TResult>(Action<EffectBuilder<TResult>> builderProvider)
        {
            var cbc = BuilderFactory.EffectBuilder<TResult>(); // Input template data here

            builderProvider(cbc);
            
            _card.Effects.Add(cbc.Build());

            return this;
        }
        
        public CardBuilder Effect<TTarget, TResult>(
            int targetIndex,
            Func<TTarget, IGameState, TResult> resolveEffect,
            Func<TTarget, IGameState, string> rulesText=null
            )
            where  TTarget : IGamePiece
        {
            return EffectBuilder<TTarget, TResult>(provider => 
                provider
                    .TargetIndex(targetIndex)
                    .Resolve(resolveEffect)
                    .Text(rulesText ?? ((_, _) => string.Empty) )
                );
        }
        
        public CardBuilder Effect<TResult>(
            Func<IGameState, TResult> resolveEffect,
            Func<IGameState, string> rulesText=null
        )
        {
            return EffectBuilder<TResult>(provider => 
                provider
                    .Resolve(resolveEffect)
                    .Text(rulesText ?? (_ => string.Empty) )
            );
        }

        public ICard Build()
        {
            var targetCount = _card.Targets.Count;
            
            foreach (var effect in _card.Effects)
            {
                string textLine;

                try
                {
                    textLine = effect.CallTextMethod(null, null);
                }
                catch
                {
                    textLine = "Text error: Additional state needed";
                }
                
                AddTextLine(textLine);

                if (effect.TargetIndex >= targetCount)
                {
                    throw new InvalidTargetIndexException(effect.TargetIndex, targetCount);
                }
            }

            _cardData.RulesText = _rulesTextBuilder.ToString();
            
            Console.WriteLine($"Creating card {_cardData.Name}");

            return _card;
        }
    }
}
