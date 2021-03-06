﻿using Archetype.Core;
using System;

namespace Archetype.CardBuilder
{
    public abstract class CardBuilder<T, TBuilder> : BaseBuilder<T>
        where T : CardData, new()
        where TBuilder : CardBuilder<T, TBuilder>
    {
        protected CardBuilder(T template)
        {
            template ??= new();

            Construction = template with { Id = Guid.NewGuid() };
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

        public TBuilder Red() => Color(CardColor.Red);
        public TBuilder Blue() => Color(CardColor.Blue);
        public TBuilder Black() => Color(CardColor.Black);
        public TBuilder White() => Color(CardColor.White);
        public TBuilder Green() => Color(CardColor.Green);

        public TBuilder Text(string text)
        {
            Construction.RulesText = text;

            return this as TBuilder;
        }

        public TBuilder Art(string link)
        {
            Construction.ImagePath = link;

            return this as TBuilder;
        }

        protected override void PreBuild()
        {
            Console.WriteLine($"Creating card {Construction.Name}");
        }
    }
}
