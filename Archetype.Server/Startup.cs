using Aqua.EnumerableExtensions;
using Archetype.CardBuilder;
using Archetype.CardBuilder.Extensions;
using Archetype.Game.Actions;
using Archetype.Game.Payloads;
using Archetype.Game.Payloads.Pieces;
using Archetype.Server.Schema;
using MediatR;
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
            // Add GraphQL Services
            services
                .AddMediatR(typeof(PlayCardAction).Assembly)
                .AddGraphQLServer()
                .AddQueryType<Queries>()
                .AddMutationType<Mutations>()
                .AddSubscriptionType<Subscriptions>()
                .AddInMemorySubscriptions()
                ;
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app
                .UseWebSockets()
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
                        .Target<IUnit>()
                        .Attack(5, 0)
                        .Effect<int>(
                            resolveEffect: context =>
                            {
                                context.GameState.Player.Hand.Cards.ForEach((card, i) => card.AffectSomehow(i));
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
                        .Target<IUnit>()
                        .Effect<IUnit, int>(
                            targetIndex: 0,
                            resolveEffect: context => context.Target.Attack(context.GameState.Player.Resources),
                            rulesText: context => $"Deal {context.GameState.Player.Resources}")
                        .Art("other")
                )
                .Card(builder =>
                    builder
                        .Red()
                        .Name("Slap cards")
                        .Cost(1)
                        .Target<IZone<ICard>>()
                        .Effect<IZone<ICard>, int>(
                            targetIndex: 0,
                            resolveEffect: context =>
                            {
                                context.Target.Contents.ForEach((card, i) => card.AffectSomehow(i));
                                return 0;
                            },
                            rulesText: context => $"Deal {context.GameState.Player.Resources}")
                        .Art("other")
                )
                .Card(builder =>
                    builder
                        .Blue()
                        .Name("Slap all")
                        .Cost(1)
                        .Effect<int>(
                            resolveEffect: context =>
                            {
                                context.GameState.Player.Hand.Cards.ForEach((card, i) => card.AffectSomehow(i));
                                return 0;
                            },
                            rulesText: (state) => $"Affect all cards in player's hand somehow")
                        .Art("other")
                )
                .Build();
        }
    }
}