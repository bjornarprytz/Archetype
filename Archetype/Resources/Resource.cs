using System;

namespace Archetype
{
    public abstract class Resource
    {
        public abstract int Value { get; set; }

        public Resource() { }


        public virtual bool CanAfford<C>(Payment<C> cost) where C : Resource
        {
            if (cost.Amount == 0) return true;

            return Value >= cost.Amount;
        }

        public virtual bool TryPay<C>(Payment<C> cost) where C : Resource
        {
            if (!CanAfford<C>(cost)) return false;

            Value -= cost.Amount;

            return true;
        }

        public virtual void ForcePay<C>(Payment<C> cost) where C : Resource
        {
            Value -= cost.Amount;
        }

        public virtual void Gain<C>(Payment<C> payment) where C : Resource
        {
            Value += payment.Amount;
        }
    }
}
