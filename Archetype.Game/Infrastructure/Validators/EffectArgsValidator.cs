using Archetype.Core;
using FluentValidation;

namespace Archetype.Game.Infrastructure.Validators
{
    public class EffectArgsValidator : AbstractValidator<IEffectArgs>
    {
        public EffectArgsValidator()
        {
            RuleFor(x => x.Targets)
                .NotNull();

            RuleForEach(x => x.Targets)
                .Must((args, target) => args.AllowedTypes.Contains(target.GetType()))
                .WithMessage("Not all targets are of allowed types!");
            
            RuleFor(x => x.Targets.Count)
                .LessThanOrEqualTo(x => x.MaxTargets)
                .GreaterThanOrEqualTo(x => x.MinTargets);
        }
    }
}