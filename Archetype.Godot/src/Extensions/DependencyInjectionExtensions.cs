using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Archetype.Godot.Card;
using Archetype.Godot.Clearing;
using Archetype.Godot.Enemy;
using Archetype.Godot.Infrastructure;
using Archetype.Godot.StateMachine;
using Archetype.Godot.Targeting;
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
                        ("res://scn//card/cardnode.tscn")
                    .Add<ClearingNode>
                        ("res://scn/clearing.tscn")
                    .Add<EnemyNode>
                        ("res://scn/enemy.tscn")
                    .Add<GameLoader>
                        ("res://scn/game.tscn")
                    .Add<MainMenuController>
                        ("res://scn/mainmenu.tscn")
                    .Add<TargetingArrow>
                        ("res://scn/targetingarrow.tscn")
                ));
    }
    
    public static IServiceCollection AddStates(this IServiceCollection serviceCollection)
    {
        var types = typeof(IState).Assembly.GetTypes();
        
        var typesToRegister = typeof(IState).Assembly.GetAllTypesImplementing<IState>();

        foreach (var type in typesToRegister)
        {
            serviceCollection.AddTransient(type);
        }
        return serviceCollection;
    }
    
    private static IEnumerable<Type> GetAllTypesImplementing<T>(this Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(t => t.IsConcrete() && (t.IsSubclassOf(typeof(T)) || t.GetInterfaces().Contains(typeof(T))));
    }
    
    private static bool IsConcrete(this Type type) => !type.IsAbstract && !type.IsInterface;
}