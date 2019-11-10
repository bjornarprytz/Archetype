using System;
using System.Collections.Generic;

namespace Archetype
{
    public class ResourcePool
    {
        private Dictionary<Type, Currency> _balance;

        public ResourcePool()
        {
            _balance = new Dictionary<Type, Currency>();
        }

        public int Amount<C>() where C : Currency
        {
            return _balance.ContainsKey(typeof(C)) ? _balance[typeof(C)].Value : 0;
        }

        public bool CanAfford<C>(Payment<C> cost) where C : Currency
        {
            if (cost.Amount == 0) return true;

            if (!_balance.ContainsKey(typeof(C))) return false;

            return _balance[typeof(C)].Value >= cost.Amount;
        }

        public bool TryPay<C>(Payment<C> cost) where C : Currency
        {
            if (!CanAfford<C>(cost)) return false;

            _balance[typeof(C)].Pay(cost);

            return true;
        }

        public void ForcePay<C>(Payment<C> cost) where C : Currency, new()
        {
            if (!_balance.ContainsKey(typeof(C)))
                _balance.Add(typeof(C), new C());


            _balance[typeof(C)].Pay(cost);
        }

        public void Gain<C>(Payment<C> payment) where C : Currency, new()
        {
            if (!_balance.ContainsKey(typeof(C))) _balance.Add(typeof(C), new C());
            else _balance[typeof(C)].Gain(payment);
        }
    }
}
