using System;
using Archetype.Game.Attributes;
using Archetype.Game.Factory;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Proto;
using Archetype.View.Atoms;
using Archetype.View.Atoms.MetaData;

namespace Archetype.Game.Payloads.Atoms.Base
{
    public interface IUnit : IZoned<IUnit>, IUnitFront
    {
        [Keyword("Attack")]
        IResult<IUnit, int> Attack(int strength);
        [Keyword("Heal")]
        IResult<IUnit, int> Heal(int strength);

        [Keyword("Kill")]
        IResult<IUnit, int> Kill();
    }

    public abstract class Unit : Piece<IUnit>, IUnit
    {
        protected Unit(IUnitProtoData protoData, IGameAtom owner) : base(owner)
        {
            Name = protoData.Name;
            Health = MaxHealth = protoData.Health;
            Defense = MaxDefense = protoData.Defense;
        }

        public abstract UnitMetaData BaseMetaData { get; }
        public int MaxHealth { get; }
        public int Health { get; private set; }
        
        public int MaxDefense { get; }
        public int Defense { get; private set; }

        public IResult<IUnit, int> Attack(int strength)
        {
            var potentialDamage = Health;

            var actualDamage = Math.Min(potentialDamage, strength);
            
            Health -= actualDamage;

            return ResultFactory.Create(this, actualDamage);
        }

        public IResult<IUnit, int> Heal(int strength)
        {
            var potentialHeal = MaxHealth - Health;

            var actualHeal = Math.Min(potentialHeal, strength);
            
            Health += actualHeal;

            return ResultFactory.Create(this, actualHeal);
        }

        public IResult<IUnit, int> Kill()
        {
            var previousHealth = Health;
            
            Health = 0;

            return ResultFactory.Create(this, previousHealth);
        }

        protected override IUnit Self => this;
    }
}