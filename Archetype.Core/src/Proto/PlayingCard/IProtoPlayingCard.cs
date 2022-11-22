namespace Archetype.Core.Proto;

public interface IProtoPlayingCard : IProtoData
{
    public string ImageUri { get; }
    public string SetName { get; }
    public CardRarity Rarity { get; }
    public CardType Type { get; }
    public string SubType { get; }
    public string RulesText { get; }
    public CardColor Color { get; }
    public int Resources { get; }
}