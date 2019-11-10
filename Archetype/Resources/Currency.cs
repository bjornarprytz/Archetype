using System;

namespace Archetype
{
    public abstract class Currency
    {
        public virtual int Value
        {
            get { return _val; }
            set { _val = IsCapped ? Math.Min(UpperCap, Math.Max(LowerCap, value)) : value; }
        }
        private int _val;
        public virtual bool IsCapped => UpperCap != LowerCap && UpperCap > LowerCap;
        public int UpperCap { get; set; }
        public int LowerCap { get; set; }

        public Currency() { }

        internal virtual void Gain<C>(Payment<C> payment) where C : Currency
        {
            Value += payment.Amount;
        }
        internal virtual void Pay<C>(Payment<C> payment) where C : Currency
        {
            Value -= payment.Amount;
        }
    }
}
