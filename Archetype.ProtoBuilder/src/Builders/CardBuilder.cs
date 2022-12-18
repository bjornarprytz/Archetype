using Archetype.Components.Protos;
using Archetype.Core;

namespace Archetype.Components.Builders;

internal abstract class CardBuilder<T> : ICardBuilder
    where T : ProtoCard
{
    protected abstract T Proto { get; }

    public void SetName(string name)
    {
        Proto.Meta = Proto.Meta with
        {
            Name = name
        };
    }
    public void SetRarity(CardRarity rarity)
    {
        Proto.Meta = Proto.Meta with
        {
            Rarity = rarity
        };
    }
    public void SetCost(int cost)
    {
        Proto.Stats = Proto.Stats with
        {
            Cost = cost
        };
    }

    public void SetColor(CardColor color)
    {
        Proto.Stats = Proto.Stats with
        {
            Color = color
        };
    }

    public void SetArt(string link)
    {
        Proto.Meta = Proto.Meta with
        {
            ImageUri = link
        };
    }

    public void SetType(CardType type)
    {
        Proto.Stats = Proto.Stats with
        {
            Type = type
        };
    }
    public void SetSubtype(string subType)
    {
        Proto.Stats = Proto.Stats with
        {
            SubType = subType
        };
    }

    public void SetResources(int resources)
    {
        Proto.Stats = Proto.Stats with
        {
            Resources = resources
        };
    }

    public void SetCardSet(string setName)
    {
        Proto.Meta = Proto.Meta with
        {
            SetName = setName
        };
    }

    public void OverrideRulesText(string text)
    {
        Proto.Meta = Proto.Meta with
        {
            StaticRulesText = text
        };
    }
}