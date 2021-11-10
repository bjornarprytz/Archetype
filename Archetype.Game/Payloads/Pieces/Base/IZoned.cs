namespace Archetype.Game.Payloads.Pieces.Base
{
    public interface IZoned<T>
        where  T : IGameAtom, IZoned<T>
    {
        // TODO: Expose Observable ZoneTransition here
        public IZone<T> CurrentZone { get; set; }
    }
}