using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CatalogContext;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Models;
using CatalogContext.Data;

namespace Catalog.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args)
				 .MigrateDbContext<CatalogDbContext>((context, services) =>
				 {
					 var env = services.GetService<IHostingEnvironment>();
					 var settings = services.GetService<IOptions<CatalogSettings>>();
					 var logger = services.GetService<ILogger<CatalogContextSeed>>();

					 new CatalogContextSeed()
					 .SeedAsync(context, env, settings, logger)
					 .Wait();

				 })
				.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
