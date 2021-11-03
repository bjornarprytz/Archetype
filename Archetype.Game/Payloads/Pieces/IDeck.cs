namespace Archetype.Game.Payloads.Pieces
{
    public interface IDeck : IZone
    {
        ICard Draw();
        void Shuffle();
        void PutCardOnTop(ICard card);
        void PutCardOnBottom(ICard card);
        int Mill(int strength);
    }
}
