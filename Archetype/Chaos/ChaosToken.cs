namespace Archetype
{
    public class ChaosToken
    {
        // But what does it do?

        // Effects on the Unit that draws it
        // Could be the card's responsibility to resolve effects depending on the type of token.

        public Alignment Alignment { get; }

        public ChaosToken(Alignment alignment)
        {
            Alignment = alignment;
        }
    }
}
