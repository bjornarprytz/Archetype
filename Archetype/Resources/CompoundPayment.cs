using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public struct CompoundPayment
    {
        public List<Payment> Payments { get; set; }

        public CompoundPayment(IEnumerable<Payment> payments)
        {
            Payments = new List<Payment>(payments);
        }
    }
}
