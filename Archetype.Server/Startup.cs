using Archetype.Builder.Extensions;
using Archetype.Design.Extensions;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Server.Extensions;
using HotChocolate.Subscriptions;
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
            services
                .AddBuilders()
                .AddArchetype()
                .AddDesign()
                .AddGraphQLServer()
                .AddMutationConventions()
                .RegisterService<ITopicEventSender>()
                .RegisterService<IGameState>()
                .RegisterService<IProtoPool>()

                .AddQueryType<Queries>()
                .AddMutationType<Mutations>()
                .AddSubscriptionType<Subscriptions>()
                .AddInMemorySubscriptions()
                .AddLocalTypes(typeof(CardType).Assembly)
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
    }
}