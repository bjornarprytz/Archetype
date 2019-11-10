using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public abstract class Payment
    {
        public abstract Type Currency { get; }
        public virtual int Amount { get; protected set; }

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
