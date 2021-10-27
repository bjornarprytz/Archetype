using Aqua.EnumerableExtensions;
using Archetype.CardBuilder;
using Archetype.CardBuilder.Extensions;
using Archetype.Game.Payloads;
using Archetype.Game.Payloads.Pieces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Archetype.Server
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(_ => BuildDummySet());
            services.AddSingleton<IGameState, GameState>();
            
            // Add GraphQL Services
            services
                .AddGraphQLServer()
                .AddQueryType<Query>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapGraphQL();
                });
        }

        private static ICardSet BuildDummySet()
        {
            return SetBuilder.CreateSet("TestSet")
                .Card(builder =>
                    builder
                        .Name("Slap heal")
                        .Black()
                        .Cost(4)
                        .Target<IEnemy>()
                        .Attack(5, 0)
                        .Effect<int>(
                            resolveEffect: (state) =>
                            {
                                state.Player.Hand.Cards.ForEach((card, i) => card.AffectSomehow(i));
                                return 0;
                            },
                            rulesText: (state) => $"Affect all cards in player's hand somehow")
                        .Art("asd")
                )
                .Card(builder =>
                    builder
                        .Green()
                        .Name("Resource slap")
                        .Cost(3)
                        .Target<IEnemy>()
                        .Effect<IEnemy, int>(
                            targetIndex: 0,
                            resolveEffect: (enemy, state) => enemy.Attack(state.Player.Resources),
                            rulesText: (enemy, state) => $"Deal {state.Player.Resources}")
                        .Art("other")
                )
                .Card(builder =>
                    builder
                        .Red()
                        .Name("Slap cards")
                        .Cost(1)
                        .Target<IZone>()
                        .Effect<IZone, int>(
                            targetIndex: 0,
                            resolveEffect: (zone, state) =>
                            {
                                zone.Cards.ForEach((card, i) => card.AffectSomehow(i));
                                return 0;
                            },
                            rulesText: (zone, state) => $"Deal {state.Player.Resources}")
                        .Art("other")
                )
                .Card(builder =>
                    builder
                        .Blue()
                        .Name("Slap all")
                        .Cost(1)
                        .Effect<int>(
                            resolveEffect: (state) =>
                            {
                                state.Player.Hand.Cards.ForEach((card, i) => card.AffectSomehow(i));
                                return 0;
                            },
                            rulesText: (state) => $"Affect all cards in player's hand somehow")
                        .Art("other")
                )
                .Build();
        }
    }
}