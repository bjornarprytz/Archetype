using Archetype.Game.Payloads.Context;

namespace Archetype.Game.Payloads.Pieces.Base
{
    public interface IZoned<T> : IGameAtom
        where  T : IGameAtom, IZoned<T>
    {
        // TODO: Expose Observable ZoneTransition here
        public IZone<T> CurrentZone { get; set; }

        IEffectResult<IZoned<T>, IZone<T>> MoveTo(IZone<T> zone);
    }
}