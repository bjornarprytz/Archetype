using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public struct Payment<C> where C : Resource
    {
        public int Amount { get; private set; }

        public Payment(int amount)
        {
            Amount = amount;
        } 
    }
}
