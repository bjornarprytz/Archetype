using System;

namespace Archetype.Game.Payloads.Metadata
{
    public interface ITarget
    {
        Type TargetType { get; }
        bool ValidateContext(ITargetValidationContext context);
    }
}