using Archetype.Core.Play;
using Archetype.Core.Proto;
using Archetype.View.Atoms;
using Archetype.View.Atoms.MetaData;

namespace Archetype.Core.Atoms.Base;

public interface IUnit : IPiece<IUnit>, IUnitFront
{
    IEffectResult<IUnit, int> Attack(int strength);
    IEffectResult<IUnit, int> Heal(int strength);
    IEffectResult<IUnit, int> Kill();
}

internal abstract class Unit : Piece<IUnit>, IUnit
{
    protected Unit(IUnitProtoData protoData) : base(protoData.Name)
    {
        Health = MaxHealth = protoData.Health;
        Defense = MaxDefense = protoData.Defense;
    }

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