using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Game.Payloads.Pieces
{
    public interface IUnit : IGamePiece
    {
        IUnitProtoData ProtoData { get; }
        IDeck Deck { get; }
        
        IZone CurrentZone { get; set; }
        int Health { get; }

        int Attack(int strength);

        int Heal(int strength);
    }
    
    public class Unit : GamePiece, IUnit
    {
        public Unit(IUnitProtoData protoData, IGamePiece owner) : base(owner)
        {
            ProtoData = protoData;
            Deck = new Deck(this);

            Health = ProtoData.Health;
        }

        public IZone CurrentZone { get; set; }

        public IUnitProtoData ProtoData { get; }
        public IDeck Deck { get; }

        public int Health { get; private set; }

        public int Attack(int strength)
        {
            Health -= strength;

            return strength;
        }

        public int Heal(int strength)
        {
            Health += strength;

            return strength;
        }
    }
}