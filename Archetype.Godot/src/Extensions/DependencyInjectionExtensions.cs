using System;
using Archetype.Godot.Card;
using Archetype.Godot.Infrastructure;
using Godot;
using Microsoft.Extensions.DependencyInjection;

namespace Archetype.Godot.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddSceneFactories(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddSingleton<ISceneFactory, SceneFactory>()
            .AddSingleton<IPackedSceneConfiguration>(
                PackedSceneConfiguration.Create(config => config
                    .Add<CardNode>("res://scn/card.tscn")
                ));
    }
}