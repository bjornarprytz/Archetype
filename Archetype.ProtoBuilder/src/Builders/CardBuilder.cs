using Archetype.Components.Protos;
using Archetype.Core;

namespace Archetype.Components.Builders;

internal abstract class CardBuilder<T> : ICardBuilder
    where T : ProtoCard
{
    protected abstract T Proto { get; }

    public void WithName(string name)
    {
        Proto.Name = name;
    }
    public void WithRarity(CardRarity rarity)
    {
        Proto.Rarity = rarity;
    }
    public void WithCost(int cost)
    {
        Proto.Cost = cost;
    }

    public void WithColor(CardColor color)
    {
        Proto.Color = color;
    }

    public void WithArt(string link)
    {
        Proto.ImageUri = link;
    }

    public void WithType(CardType type)
    {
        Proto.Type = type;
    }
    public void WithSubtype(string subtype)
    {
        Proto.SubType = subtype;
    }

    public void WithResources(int resources)
    {
        Proto.Resources = resources;
    }

    public void FromSet(string setName)
    {
        Proto.SetName = setName;
    }

    public void OverrideRulesText(string text)
    {
        Proto.RulesText = text;
    }
}