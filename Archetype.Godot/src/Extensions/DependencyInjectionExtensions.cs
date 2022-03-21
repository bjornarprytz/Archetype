using Archetype.Godot.Card;
using Archetype.Godot.Clearing;
using Archetype.Godot.Enemy;
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
            .AddSingleton<IEnemyFactory, EnemyFactory>()
            
            .AddSingleton<ISceneFactory, SceneFactory>()

            .AddSingleton<IPackedSceneConfiguration>(
                PackedSceneConfiguration.Create(config => config
                    .Add<CardNode>
                        ("res://scn/card.tscn")
                    .Add<ClearingNode>
                        ("res://scn/clearing.tscn")
                    .Add<EnemyNode>
                        ("res://scn/enemy.tscn")
                    .Add<GameLoader>
                        ("res://scn/game.tscn")
                    .Add<MainMenuController>
                        ("res://scn/mainmenu.tscn")
                ));
    }
}