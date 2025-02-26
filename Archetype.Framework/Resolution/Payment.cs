using System.Reflection;
using Archetype.Framework.Core;
using Archetype.Framework.Events;

namespace Archetype.Framework.Resolution;

public interface IPayment
{
    // TODO: Figure out how to define payments and costs
    /*
     * It would be nice if any state change could be a payment,
     * but we need a nice way to define the check for the payment.
     *
     * I should probably reconsider these static effect resolvers too.
     * I could test having the effects implement an interface while still being static.
     */
    
    string Keyword { get; }
    Func<IResolutionContext, IEvent> BindResolver(CostProto costProto);
    Func<IResolutionContext, bool> BindPaymentCheck(CostProto costProto);
}