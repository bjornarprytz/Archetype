using System;
using Archetype.Dto.MetaData;
using Archetype.Game.Attributes;
using Archetype.Game.Payloads.Pieces.Base;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Game.Payloads.Pieces
{
    [Target("Unit")]
    public interface IUnit : IGameAtom, IZoned<IUnit>
    {
        Guid ProtoGuid { get; }
        IDeck Deck { get; }
        UnitMetaData MetaData { get; }
        
        int MaxHealth { get; }
        int Health { get; }
        
        [Verb("Attack")]
        int Attack(int strength);
        [Verb("Heal")]
        int Heal(int strength);
    }
    
    public class Unit : Piece<IUnit>, IUnit
    {
        public Unit(IUnitProtoData protoData, IGameAtom owner) : base(owner)
        {
            ProtoGuid = protoData.Guid;
            Deck = new Deck(this);

            Health = MaxHealth = protoData.Health;
            MetaData = protoData.MetaData;
        }

        public Guid ProtoGuid { get; }
        public IDeck Deck { get; }
        public UnitMetaData MetaData { get; }

        public int MaxHealth { get; }
        public int Health { get; private set; }

        public int Attack(int strength)
        {
            var potentialDamage = Health;

            var actualDamage = Math.Min(potentialDamage, strength);
            
            Health -= actualDamage;

            return actualDamage;
        }

        public int Heal(int strength)
        {
            var potentialHeal = MaxHealth - Health;

            var actualHeal = Math.Min(potentialHeal, strength);
            
            Health += actualHeal;

            return actualHeal;
        }
    }
}