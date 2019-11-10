using System;

namespace Archetype
{
    public abstract class Resource
    {
        public virtual int Value { get; set; }

        public Resource(int initialValue) { Value = initialValue; }
        public Resource() { }


        public virtual bool CanAfford(Payment cost)
        {
            if (cost.Amount == 0) return true;

            return Value >= cost.Amount;
        }

        public virtual bool TryPay(Payment cost)
        {
            if (!CanAfford(cost)) return false;

            Value -= cost.Amount;

            return true;
        }

        public virtual void ForcePay(Payment cost)
        {
            Value -= cost.Amount;
        }

        public virtual void Gain(Payment payment)
        {
            Value += payment.Amount;
        }
    }
}
