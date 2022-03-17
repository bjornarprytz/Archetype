using Archetype.Godot.Card;
using Archetype.Godot.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Archetype.Godot.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddSceneFactories(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddSingleton<ICardFactory, CardFactory>()
            .AddSingleton<IClearingFactory, ClearingFactory>()
            

            .AddSingleton<ISceneFactory, SceneFactory>()
            .AddSingleton<IPackedSceneConfiguration>(
                PackedSceneConfiguration.Create(config => config
                    .Add<CardNode>
                        ("res://scn/card.tscn")
                    .Add<GameLoader>
                        ("res://scn/game.tscn")
                    .Add<MainMenuController>
                        ("res://scn/mainmenu.tscn")
                    .Add<Clearing>
                        ("res://scn/clearing.tscn")
                ));
    }
}