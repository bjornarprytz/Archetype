using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Archetype.Builder.Builders.Base;
using Archetype.Builder.Factory;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Context.Effect;
using Archetype.Game.Payloads.Context.Effect.Base;
using Archetype.Game.Payloads.MetaData;
using Archetype.Game.Payloads.Pieces.Base;
using Archetype.Game.Payloads.Primitives;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Builder.Builders
{
    public interface ICardBuilder : IBuilder<ICardProtoData>
    {
        ICardBuilder MetaData(CardMetaData metaData);
        ICardBuilder Name(string name);
        ICardBuilder Rarity(CardRarity rarity);
        ICardBuilder Cost(int cost);
        ICardBuilder Range(int range);
        ICardBuilder Color(CardColor color);
        ICardBuilder Art(string link);

        ICardBuilder Target<TTarget>(Func<ITargetValidationContext<TTarget>, bool> validateTarget = null)
            where TTarget : IGameAtom;

        ICardBuilder Targets<TT1, TT2>(
            Func<ITargetValidationContext<TT1>, bool> validateTarget1 = null,
            Func<ITargetValidationContext<TT2>, bool> validateTarget2 = null
        )
            where TT1 : IGameAtom
            where TT2 : IGameAtom;

        ICardBuilder Targets<TT1, TT2, TT3>(
            Func<ITargetValidationContext<TT1>, bool> validateTarget1 = null,
            Func<ITargetValidationContext<TT2>, bool> validateTarget2 = null,
            Func<ITargetValidationContext<TT3>, bool> validateTarget3 = null
        )
            where TT1 : IGameAtom
            where TT2 : IGameAtom
            where TT3 : IGameAtom;

        ICardBuilder Effect<TTarget>(Action<ICardEffectBuilder<TTarget>> builderProvider) where TTarget : IGameAtom;
        ICardBuilder Effect(Action<ICardEffectBuilder> builderProvider);
        ICardBuilder Effect<TTarget>(Expression<Func<IEffectContext<TTarget>, IResult>> resolveEffect, int targetIndex = -1) where TTarget : IGameAtom;
        ICardBuilder Effect(Expression<Func<IContext, IResult>> resolveEffect);
    }

    public class CardBuilder : ProtoBuilder<ICardProtoData>, ICardBuilder
    {
        private readonly IBuilderFactory _builderFactory;
        private readonly CardProtoData _cardProtoData;

        private readonly List<ITarget> _targets = new();
        private readonly List<IEffect<ICardContext>> _effects = new();

        public CardBuilder(IBuilderFactory builderFactory)
        {
            _builderFactory = builderFactory;
            _cardProtoData = new CardProtoData(_targets, _effects);
        }

        public ICardBuilder MetaData(CardMetaData metaData)
        {
            _cardProtoData.MetaData = metaData;

            return this;
        }

        public ICardBuilder Name(string name)
        {
            _cardProtoData.Name = name;

            return this;
        }

        public ICardBuilder Rarity(CardRarity rarity)
        {
            _cardProtoData.MetaData = _cardProtoData.MetaData with { Rarity = rarity };

            return this;
        }

        public ICardBuilder Cost(int cost)
        {
            _cardProtoData.Cost = cost;

            return this;
        }
        
        public ICardBuilder Range(int range)
        {
            _cardProtoData.Range = range;

            return this;
        }

        public ICardBuilder Color(CardColor color)
        {
            _cardProtoData.MetaData = _cardProtoData.MetaData with { Color = color };

            return this;
        }

        public ICardBuilder Art(string link)
        {
            _cardProtoData.MetaData = _cardProtoData.MetaData with { ImageUri = link };

            return this;
        }

        public ICardBuilder Target<TTarget>(Func<ITargetValidationContext<TTarget>, bool> validateTarget=null)
            where TTarget : IGameAtom
        {
            _targets.Add(new Target<TTarget> { Validate = validateTarget });

            return this;
        }
        public ICardBuilder Targets<TT1, TT2>(
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
        public ICardBuilder Targets<TT1, TT2, TT3>(
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

        public ICardBuilder Effect<TTarget>(Action<ICardEffectBuilder<TTarget>> builderProvider)
            where  TTarget : IGameAtom
        {
            var cbc = _builderFactory.Create<ICardEffectBuilder<TTarget>>();

            builderProvider(cbc);
            
            _effects.Add(cbc.Build());

            return this;
        }
        
        public ICardBuilder Effect(Action<ICardEffectBuilder> builderProvider)
        {
            var cbc = _builderFactory.Create<ICardEffectBuilder>();

            builderProvider(cbc);
            
            _effects.Add(cbc.Build());

            return this;
        }
        
        public ICardBuilder Effect<TTarget>(
            Expression<Func<IEffectContext<TTarget>, IResult>> resolveEffect,
            int targetIndex=-1
            )
            where  TTarget : IGameAtom
        {
            if (!_targets.Any(t => t.TargetType.IsAssignableTo(typeof(TTarget))))
            {
                _targets.Add(new Target<TTarget>());
                targetIndex = _targets.Count - 1;
            }
            
            return Effect<TTarget>(provider => 
                provider
                    .TargetIndex(targetIndex)
                    .Resolve(resolveEffect)
                );
        }
        
        public ICardBuilder Effect(
            Expression<Func<IContext, IResult>> resolveEffect
        )
        {
            return Effect(provider => 
                provider
                    .Resolve(resolveEffect)
            );
        }

        protected override ICardProtoData BuildInternal()
        {
            Console.WriteLine($"Creating card {_cardProtoData.Name}");

            return _cardProtoData;
        }
    }
}
