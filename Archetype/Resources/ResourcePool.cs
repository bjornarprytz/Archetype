using System;
using System.Collections.Generic;
using System.Linq;

namespace Archetype
{
    public class ResourcePool
    {
        private Dictionary<Type, Resource> _balance;

        public ResourcePool()
        {
            _balance = new Dictionary<Type, Resource>();
        }
        public void AddResource(Resource resource)
        {
            _balance.Add(resource.GetType(), resource); // TODO: Figure out what to do if key exists (overwrite or abort?)
        }

        public int Amount<C>() where C : Resource
        {
            return _balance.ContainsKey(typeof(C)) ? _balance[typeof(C)].Value : 0;
        }

        public bool CanAfford(CompoundPayment cost) => cost.Payments.All(p => CanAfford(p));
        public bool TryPay(CompoundPayment cost) => cost.Payments.All(p => TryPay(p));
        public bool Gain(CompoundPayment cost) => cost.Payments.All(p => Gain(p));
        public bool ForcePay(CompoundPayment cost) => cost.Payments.All(p => ForcePay(p));

        public bool CanAfford(Payment cost)
        {
            if (!_balance.ContainsKey(cost.Currency)) return false;

            return _balance[cost.Currency].CanAfford(cost);
        }

        public bool TryPay(Payment cost)
        {
            if (!_balance.ContainsKey(cost.Currency)) return false;

            return _balance[cost.Currency].TryPay(cost);
        }

        public bool ForcePay(Payment cost)
        {
            if (!_balance.ContainsKey(cost.Currency)) return false;


            _balance[cost.Currency].ForcePay(cost);

            return true;
        }

        public bool Gain(Payment payment)
        {
            if (!_balance.ContainsKey(payment.Currency)) return false;


            _balance[payment.Currency].Gain(payment);

            return true;
        }
    }
}
