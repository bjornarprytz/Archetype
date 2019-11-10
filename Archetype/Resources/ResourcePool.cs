using System;
using System.Collections.Generic;

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

        public bool CanAfford<C>(Payment<C> cost) where C : Resource
        {
            if (!_balance.ContainsKey(typeof(C))) return false;

            return _balance[typeof(C)].CanAfford(cost);
        }

        public bool TryPay<C>(Payment<C> cost) where C : Resource
        {
            if (!_balance.ContainsKey(typeof(C))) return false;

            return _balance[typeof(C)].TryPay(cost);
        }

        public bool ForcePay<C>(Payment<C> cost) where C : Resource
        {
            if (!_balance.ContainsKey(typeof(C))) return false;


            _balance[typeof(C)].ForcePay(cost);

            return true;
        }

        public bool Gain<C>(Payment<C> payment) where C : Resource
        {
            if (!_balance.ContainsKey(typeof(C))) return false;


            _balance[typeof(C)].Gain(payment);

            return true;
        }
    }
}
