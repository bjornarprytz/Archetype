using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Archetype.Builder.Factory;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Context.Effect;
using Archetype.Game.Payloads.MetaData;
using Archetype.Game.Payloads.Pieces.Base;
using Archetype.Game.Payloads.Primitives;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Builder
{
    public class CardBuilder : IBuilder<ICardProtoData>
    {
        private readonly CardProtoData _cardProtoData;

        private readonly List<ITarget> _targets = new();
        private readonly List<IEffect<ICardContext>> _effects = new();

        internal CardBuilder(CardMetaData template)
        {
            _cardProtoData = new CardProtoData(_targets, _effects)
            {
                MetaData = template
            };
        }

        public CardBuilder Name(string name)
        {
            _cardProtoData.MetaData = _cardProtoData.MetaData with { Name = name };

            return this;
        }

        public CardBuilder Rarity(CardRarity rarity)
        {
            _cardProtoData.MetaData = _cardProtoData.MetaData with { Rarity = rarity };

            return this;
        }

        public CardBuilder Cost(int cost)
        {
            _cardProtoData.Cost = cost;

            return this;
        }

        public CardBuilder Color(CardColor color)
        {
            _cardProtoData.MetaData = _cardProtoData.MetaData with { Color = color };

            return this;
        }

        public CardBuilder Art(string link)
        {
            _cardProtoData.MetaData = _cardProtoData.MetaData with { ImageUri = link };

            return this;
        }

        public CardBuilder Target<TTarget>(Func<ITargetValidationContext<TTarget>, bool> validateTarget=null)
            where TTarget : IGameAtom
        {
            _targets.Add(new Target<TTarget> { Validate = validateTarget });

            return this;
        }
        public CardBuilder Targets<TT1, TT2>(
            Func<ITargetValidationContext<TT1>, bool> validateTarget1=null, 
            Func<ITargetValidationContext<TT2>, bool> validateTarget2=null
        )
            where TT1 : IGameAtom
            where TT2 : IGameAtom
        {
            
            _targets.Add(new Target<TT1> { Validate = validateTarget1 });
            _targets.Add(new Target<TT2> { Validate = validateTarget2 });

            return this;
        }
        public CardBuilder Targets<TT1, TT2, TT3>(
            Func<ITargetValidationContext<TT1>, bool> validateTarget1=null, 
            Func<ITargetValidationContext<TT2>, bool> validateTarget2=null, 
            Func<ITargetValidationContext<TT3>, bool> validateTarget3=null 
            )
            where TT1 : IGameAtom
            where TT2 : IGameAtom
            where TT3 : IGameAtom
        {
            
            _targets.Add(new Target<TT1> { Validate = validateTarget1 });
            _targets.Add(new Target<TT2> { Validate = validateTarget2 });
            _targets.Add(new Target<TT3> { Validate = validateTarget3 });

            return this;
        }
        
        public CardBuilder EffectBuilder<TTarget>(Action<CardEffectBuilder<TTarget>> builderProvider)
            where  TTarget : IGameAtom
        {
            var cbc = BuilderFactory.EffectBuilder<TTarget>();

            builderProvider(cbc);
            
            _effects.Add(cbc.Build());

            return this;
        }
        
        public CardBuilder Effect(Action<CardEffectBuilder> builderProvider)
        {
            var cbc = BuilderFactory.EffectBuilder();

            builderProvider(cbc);
            
            _effects.Add(cbc.Build());

            return this;
        }
        
        public CardBuilder Effect<TTarget>(
            Expression<Func<IEffectContext<TTarget>, IEffectResult>> resolveEffect,
            int targetIndex=-1
            )
            where  TTarget : IGameAtom
        {
            if (!_targets.Any(t => t.TargetType.IsAssignableTo(typeof(TTarget))))
            {
                _targets.Add(new Target<TTarget>());
                targetIndex = _targets.Count - 1;
            }
            
            return EffectBuilder<TTarget>(provider => 
                provider
                    .TargetIndex(targetIndex)
                    .Resolve(resolveEffect)
                );
        }
        
        public CardBuilder Effect(
            Expression<Func<IEffectContext, IEffectResult>> resolveEffect
        )
        {
            return Effect(provider => 
                provider
                    .Resolve(resolveEffect)
            );
        }

        public ICardProtoData Build()
        {
            Console.WriteLine($"Creating card {_cardProtoData.MetaData.Name}");

            return _cardProtoData;
        }
    }
}
