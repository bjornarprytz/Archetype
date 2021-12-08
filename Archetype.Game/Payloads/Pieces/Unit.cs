using System;
using Archetype.Game.Attributes;
using Archetype.Game.Factory;
using Archetype.Game.Payloads.MetaData;
using Archetype.Game.Payloads.Pieces.Base;
using Archetype.Game.Payloads.PlayContext;
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
        
        [Template("Deal {1} damage to {0}")]
        IEffectResult<IUnit> Attack(int strength);
        [Template("Heal {0} by {1}")]
        IEffectResult<IUnit> Heal(int strength);
    }
    
    public class Unit : Piece<IUnit>, IUnit
    {
        public Unit(IUnitProtoData protoData, IGameAtom owner) : base(owner)
        {
            ProtoGuid = protoData.Guid;
            Deck = new Deck(this);

            Health = MaxHealth = protoData.Health;
            Defense = MaxDefense = protoData.Defense;
            Strength = protoData.Strength;
            MetaData = protoData.MetaData;
        }

        public Guid ProtoGuid { get; }
        public IDeck Deck { get; }
        public UnitMetaData MetaData { get; }

        public int MaxHealth { get; }
        public int Health { get; private set; }
        
        public int MaxDefense { get; }
        public int Defense { get; private set; }
        
        public int Strength { get; }

        public IEffectResult<IUnit> Attack(int strength)
        {
            var potentialDamage = Health;

            var actualDamage = Math.Min(potentialDamage, strength);
            
            Health -= actualDamage;

            return ResultFactory.Create(this, actualDamage);
        }

        public IEffectResult<IUnit> Heal(int strength)
        {
            var potentialHeal = MaxHealth - Health;

            var actualHeal = Math.Min(potentialHeal, strength);
            
            Health += actualHeal;

            return ResultFactory.Create(this, actualHeal);
        }
    }
}