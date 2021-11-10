using Archetype.Game.Payloads.Pieces.Base;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Game.Payloads.Pieces
{
    public interface IUnit : IGameAtom, IZoned<IUnit>
    {
        IUnitProtoData ProtoData { get; }
        IDeck Deck { get; }
        
        int Health { get; }
        
        int Attack(int strength);
        int Heal(int strength);
    }
    
    public class Unit : Piece<IUnit>, IUnit
    {
        public Unit(IUnitProtoData protoData, IGameAtom owner) : base(owner)
        {
            ProtoData = protoData;
            Deck = new Deck(this);

            Health = ProtoData.Health;
        }

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