namespace Archetype.Game.Payloads.Pieces
{
    public interface IDeck : IZone
    {
        ICard Draw();
        void Shuffle();
    }
}
