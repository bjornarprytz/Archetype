namespace Archetype.Core
{
    public record CreatureCardData : CardData
    {
        public string Type { get; set; }

        public int Power { get; set; }
        public int Toughness { get; set; }

    }
}
