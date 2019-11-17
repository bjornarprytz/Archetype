using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archetype
{
    public struct CompoundPayment
    {
        public List<Payment> Payments => _payments.Values.ToList();

        private Dictionary<Type, Payment> _payments;
        public CompoundPayment(IEnumerable<Payment> payments)
        {
            _payments = new Dictionary<Type, Payment>();

            foreach (Payment payment in payments)
            {
                Add(payment);
            }
        }

        public void Add(Payment payment)
        {
            if (_payments.ContainsKey(payment.Currency)) _payments[payment.Currency].Compound(payment);
            else _payments.Add(payment.Currency, payment);
        }
    }
}
