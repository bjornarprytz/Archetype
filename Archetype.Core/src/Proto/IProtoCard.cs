namespace Archetype.Core.Proto;

public interface IProtoCard
{
    // SetName#Name (e.g. "Core#Bless") - used for serialization and identification
    public string Name { get; }
    public string ImageUri { get; }
    public CardRarity Rarity { get; }
    public CardType Type { get; }
    public string SubType { get; }
    public string SetName { get; }
    public string RulesText { get; }
    public CardColor Color { get; }
}