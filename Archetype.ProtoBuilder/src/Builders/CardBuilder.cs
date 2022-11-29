using Archetype.Components.Protos;
using Archetype.Core;

namespace Archetype.Components.Builders;

internal abstract class CardBuilder<T> : ICardBuilder
    where T : ProtoCard
{
    protected abstract T Proto { get; }

    public void SetName(string name)
    {
        Proto.Name = name;
    }
    public void SetRarity(CardRarity rarity)
    {
        Proto.Rarity = rarity;
    }
    public void SetCost(int cost)
    {
        Proto.Cost = cost;
    }

    public void SetColor(CardColor color)
    {
        Proto.Color = color;
    }

    public void SetArt(string link)
    {
        Proto.ImageUri = link;
    }

    public void SetType(CardType type)
    {
        Proto.Type = type;
    }
    public void SetSubtype(string subtype)
    {
        Proto.SubType = subtype;
    }

    public void SetResources(int resources)
    {
        Proto.Resources = resources;
    }

    public void SetCardSet(string setName)
    {
        Proto.SetName = setName;
    }

    public void OverrideRulesText(string text)
    {
        Proto.RulesText = text;
    }
}