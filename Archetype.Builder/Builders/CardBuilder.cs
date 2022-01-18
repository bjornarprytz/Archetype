using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Archetype.Builder.Builders.Base;
using Archetype.Builder.Exceptions;
using Archetype.Builder.Factory;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Context.Effect;
using Archetype.Game.Payloads.Context.Effect.Base;
using Archetype.Game.Payloads.Proto;
using Archetype.View.Atoms.MetaData;
using Archetype.View.Infrastructure;
using Archetype.View.Primitives;

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

        ICardBuilder Effect(Expression<Func<IContext, IResult>> resolveEffect);
    }

    internal class CardBuilder : ProtoBuilder<ICardProtoData>, ICardBuilder
    {
        private readonly CardProtoData _cardProtoData;

        private readonly List<ITargetDescriptor> _targets = new();
        private readonly List<IEffect> _effects = new();

        public CardBuilder()
        {
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
        
        public ICardBuilder Effect(
            Expression<Func<IContext, IResult>> resolveEffect
        )
        {
            _effects.Add(new Effect
            {
                ResolveExpression = resolveEffect
            });

            return this;
        }

        protected override ICardProtoData BuildInternal()
        {
            Console.WriteLine($"Creating card {_cardProtoData.Name}");

            _cardProtoData.EffectDescriptors = _effects.Select(effect => effect.CreateDescription()).ToList();

            return _cardProtoData;
        }
    }
}
