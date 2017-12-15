﻿using Funq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.Host.Handlers;
using ServiceStack.Logging;
using ServiceStack.Mvc;
using ServiceStack.OrmLite;

namespace RazorRockstars.WebHost
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseServiceStack(new AppHost {
                AppSettings = new NetCoreAppSettings(Configuration)
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.Use(new RazorHandler("/notfound"));

            //Other examples of using built-in ServiceStack Handlers as middleware
            //app.Use(new StaticFileHandler("wwwroot/img/react-logo.png").Middleware);
            //app.Use(new RequestInfoHandler().Middleware);
        }
    }

    public class AppHost : AppHostBase
    {
        public AppHost() : base("Test Razor", typeof(AppHost).Assembly)
        {
            Config.DebugMode = true;
        }

        public override void Configure(Container container)
        {
            Plugins.Add(new RazorFormat());

            container.Register<IDbConnectionFactory>(
                new OrmLiteConnectionFactory(":memory:", SqliteDialect.Provider));

            using (var db = container.Resolve<IDbConnectionFactory>().Open())
            {
                db.CreateTableIfNotExists<Rockstar>();
                db.InsertAll(RockstarsService.SeedData);
            }
        }
    }
}
