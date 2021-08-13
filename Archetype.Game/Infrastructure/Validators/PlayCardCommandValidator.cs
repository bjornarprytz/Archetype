using System.Collections.Generic;
using Archetype.Core;
using FluentValidation;

namespace Archetype.Game.Infrastructure.Validators
{
    public class PlayCardCommandValidator : AbstractValidator<PlayCardCommand>
    {
        public PlayCardCommandValidator(IEnumerable<IValidator<IEffectArgs>> effectValidators)
        {
            RuleFor(x => x.Player.Resources)
                .GreaterThanOrEqualTo(x => x.Card.Data.Cost);
            
            RuleFor(x => x.Args)
                .NotNull();

            foreach (var validator in effectValidators)
            {
                RuleForEach(x => x.Args.EffectArgs)
                    .SetValidator(validator);
            }
        }
    }
    
}