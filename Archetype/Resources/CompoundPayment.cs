using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public struct CompoundPayment
    {
        // TODO: Maybe this should be a dictionary<Type, Payment> and create logic to accumulate values from paymens of the same currency.
        public List<Payment> Payments { get; set; }

        public CompoundPayment(IEnumerable<Payment> payments)
        {
            Payments = new List<Payment>(payments);
        }
    }
}
