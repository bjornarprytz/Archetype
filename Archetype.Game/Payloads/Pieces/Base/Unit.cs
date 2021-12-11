using System;
using Archetype.Game.Attributes;
using Archetype.Game.Factory;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.MetaData;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Game.Payloads.Pieces.Base
{
    [Target("Unit")]
    public interface IUnit : IGameAtom, IZoned<IUnit>
    {
        Guid ProtoGuid { get; }
        UnitMetaData BaseMetaData { get; }
        
        int MaxHealth { get; }
        int Health { get; }
        
        int MaxDefense { get; }
        int Defense { get; }
        
        [Template("Deal {1} damage to {0}")]
        IEffectResult<IUnit, int> Attack(int strength);
        [Template("Heal {0} by {1}")]
        IEffectResult<IUnit, int> Heal(int strength);

        [Template("Kill {0}")]
        IEffectResult<IUnit, int> Kill();


    }
    
    public abstract class Unit : Piece<IUnit>, IUnit
    {
        protected Unit(IUnitProtoData protoData, IGameAtom owner) : base(owner)
        {
            ProtoGuid = protoData.Guid;

            Health = MaxHealth = protoData.Health;
            Defense = MaxDefense = protoData.Defense;
        }

        public Guid ProtoGuid { get; }
        public abstract UnitMetaData BaseMetaData { get; }
        public int MaxHealth { get; }
        public int Health { get; private set; }
        
        public int MaxDefense { get; }
        public int Defense { get; private set; }

        public IEffectResult<IUnit, int> Attack(int strength)
        {
            var potentialDamage = Health;

            var actualDamage = Math.Min(potentialDamage, strength);
            
            Health -= actualDamage;

            return ResultFactory.Create(this, actualDamage);
        }

        public IEffectResult<IUnit, int> Heal(int strength)
        {
            var potentialHeal = MaxHealth - Health;

            var actualHeal = Math.Min(potentialHeal, strength);
            
            Health += actualHeal;

            return ResultFactory.Create(this, actualHeal);
        }

        public IEffectResult<IUnit, int> Kill()
        {
            var previousHealth = Health;
            
            Health = 0;

            return ResultFactory.Create(this, previousHealth);
        }

        protected override IUnit Self => this;
    }
}