using Archetype.Rules.Actions;
using Archetype.Rules.Validation;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Archetype.Rules;

public static class DependencyInjection
{
    public static IServiceCollection AddRules(this IServiceCollection services)
    {
        services
            .AddValidatorsFromAssemblyContaining<PlayCard.Validator>()
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))
            .AddMediatR(typeof(PlayCard.Handler).Assembly);
        
        return services;
    }
}