using System;
using System.Collections.Generic;
using System.Linq;
using Archetype.Builder.Factory;
using Archetype.Dto.Composite;
using Archetype.Dto.Simple;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Builder
{
    public class CardBuilder : IBuilder<ICardProtoData>
    {
        private readonly CardProtoData _cardProtoData;

        private readonly CardMetaData _cardMetaData = new();

        private readonly List<ITarget> _targets = new();
        private readonly List<IEffect> _effects = new();

        internal CardBuilder()
        {
            _cardProtoData = new CardProtoData(Guid.NewGuid(), _cardMetaData, _targets, _effects);
        }

        public CardBuilder Name(string name)
        {
            _cardMetaData.Name = name;

            return this;
        }

        public CardBuilder Rarity(CardRarity rarity)
        {
            _cardMetaData.Rarity = rarity;

            return this;
        }

        public CardBuilder Cost(int cost)
        {
            _cardProtoData.Cost = cost;

            return this;
        }

        public CardBuilder Color(CardColor color)
        {
            _cardMetaData.Color = color;

            return this;
        }

        public CardBuilder Art(string link)
        {
            _cardMetaData.ImageUri = link;

            return this;
        }

        public CardBuilder Target<TTarget>(Func<ITargetValidationContext<TTarget>, bool> validateTarget=null)
            where TTarget : IGamePiece
        {
            _targets.Add(new Target<TTarget>(validateTarget));

            return this;
        }
        public CardBuilder Targets<TT1, TT2>(
            Func<ITargetValidationContext<TT1>, bool> validateTarget1=null, 
            Func<ITargetValidationContext<TT2>, bool> validateTarget2=null
        )
            where TT1 : IGamePiece
            where TT2 : IGamePiece
        {
            
            _targets.Add(new Target<TT1>(validateTarget1));
            _targets.Add(new Target<TT2>(validateTarget2));

            return this;
        }
        public CardBuilder Targets<TT1, TT2, TT3>(
            Func<ITargetValidationContext<TT1>, bool> validateTarget1=null, 
            Func<ITargetValidationContext<TT2>, bool> validateTarget2=null, 
            Func<ITargetValidationContext<TT3>, bool> validateTarget3=null 
            )
            where TT1 : IGamePiece
            where TT2 : IGamePiece
            where TT3 : IGamePiece
        {
            
            _targets.Add(new Target<TT1>(validateTarget1));
            _targets.Add(new Target<TT2>(validateTarget2));
            _targets.Add(new Target<TT3>(validateTarget3));

            return this;
        }
        
        public CardBuilder EffectBuilder<TTarget, TResult>(Action<EffectBuilder<TTarget, TResult>> builderProvider)
            where  TTarget : IGamePiece
        {
            var cbc = BuilderFactory.EffectBuilder<TTarget, TResult>(); // Input template data here

            builderProvider(cbc);
            
            _effects.Add(cbc.Build());

            return this;
        }
        
        public CardBuilder Effect<TResult>(Action<EffectBuilder<TResult>> builderProvider)
        {
            var cbc = BuilderFactory.EffectBuilder<TResult>(); // Input template data here

            builderProvider(cbc);
            
            _effects.Add(cbc.Build());

            return this;
        }
        
        public CardBuilder Effect<TTarget, TResult>(
            int targetIndex,
            Func<IEffectResolutionContext<TTarget>, TResult> resolveEffect,
            Func<IEffectResolutionContext<TTarget>, string> rulesText=null
            )
            where  TTarget : IGamePiece
        {
            return EffectBuilder<TTarget, TResult>(provider => 
                provider
                    .TargetIndex(targetIndex)
                    .Resolve(resolveEffect)
                    .Text(rulesText ?? (_ => string.Empty) )
                );
        }
        
        public CardBuilder Effect<TResult>(
            Func<IEffectResolutionContext, TResult> resolveEffect,
            Func<IEffectResolutionContext, string> rulesText=null
        )
        {
            return Effect<TResult>(provider => 
                provider
                    .Resolve(resolveEffect)
                    .Text(rulesText ?? (_ => string.Empty) )
            );
        }

        public ICardProtoData Build()
        {
            var targetCount = _targets.Count;

            foreach (var effect in _cardProtoData.Effects.Where(effect => effect.TargetIndex >= targetCount))
            {
                throw new InvalidTargetIndexException(effect.TargetIndex, targetCount);
            }
            
            Console.WriteLine($"Creating card {_cardProtoData.MetaData.Name}");

            return _cardProtoData;
        }
    }
}
