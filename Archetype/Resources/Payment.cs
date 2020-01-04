using System;

namespace Archetype
{
    public abstract class Payment
    {
        public abstract Type Currency { get; }
        public virtual int Amount { get; protected set; }

        public virtual void Compound(Payment other)
        {
            if (Currency != other.Currency) throw new Exception("Cannot compound different currencies");

            Amount += other.Amount;
        }

        public virtual void Detract(Payment other)
        {
            if (Currency != other.Currency) throw new Exception("Cannot detract between different currencies");

            Amount -= other.Amount;
        }

    }

    public class Payment<C> : Payment where C : Resource
    {
        public override Type Currency => typeof(C);
        public Payment(int amount) 
        {
            Amount = amount;
        }
    }
}
