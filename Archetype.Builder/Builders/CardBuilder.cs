﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Archetype.Builder.Builders.Base;
using Archetype.Core.Play;
using Archetype.Core.Proto;
using Archetype.View.Atoms.MetaData;
using Archetype.View.Infrastructure;
using Archetype.View.Primitives;

namespace Archetype.Builder.Builders;

public interface ICardBuilder : IBuilder<ICardProtoData>
{
    ICardBuilder MetaData(CardMetaData metaData);
    ICardBuilder Name(string name);
    ICardBuilder Rarity(CardRarity rarity);
    ICardBuilder Cost(int cost);
    ICardBuilder Range(int range);
    ICardBuilder Color(CardColor color);
    ICardBuilder Art(string link);

    ICardBuilder Effect(Expression<Func<IContext, IEffectResult>> resolveEffect);
}

internal class CardBuilder : ProtoBuilder<ICardProtoData>, ICardBuilder
{
    private readonly CardProtoData _cardProtoData;

    private readonly List<IEffect> _effects = new();

    public CardBuilder()
    {
        _cardProtoData = new CardProtoData(_effects);
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
        Expression<Func<IContext, IEffectResult>> resolveEffect
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

        _cardProtoData.GenerateDescriptors();

        return _cardProtoData;
    }
}