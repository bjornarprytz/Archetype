﻿using Archetype.Framework.State;

namespace Archetype.Framework.Core.Primitives;

public interface IPaymentContext
{
    IResolutionContext ResolutionContext { get; }
    IReadOnlyDictionary<CostType, IReadOnlyList<IAtom>> Payments { get; }
    IReadOnlyList<IKeywordInstance> Costs { get; }
}

public record PaymentContext(IResolutionContext ResolutionContext, IReadOnlyList<IKeywordInstance> Costs) : IPaymentContext
{
    public IReadOnlyDictionary<CostType, IReadOnlyList<IAtom>> Payments => ResolutionContext.Payments;
}