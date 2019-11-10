using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public struct CompoundPayment
    {
        public List<Payment<Resource>> Payments { get; set; }

        public CompoundPayment(IEnumerable<Payment<Resource>> payments)
        {
            Payments = new List<Payment<Resource>>(payments);
        }
    }
}
