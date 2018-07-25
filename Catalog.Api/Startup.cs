using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CatalogContext;
using CatalogContext.Interfaces;
using CatalogContext.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;

namespace Catalog.Api
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

			var connectionstring = Configuration.GetConnectionString("DefaultConnection");

			services.AddDbContext<CatalogDbContext>(options =>
			{
				options.UseSqlServer(connectionstring, sqlServerOptionsAction: sqlOptions => 
				{
					sqlOptions.MigrationsAssembly(typeof(CatalogDbContext).GetTypeInfo().Assembly.GetName().Name);
					//Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
					sqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
				});
				// Changing default behavior when client evaluation occurs to throw. 
				// Default in EF Core would be to log a warning when client evaluation is performed.
				options.ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning));
				//Check Client vs. Server evaluation: https://docs.microsoft.com/en-us/ef/core/querying/client-eval

			});

			// TO Avoid Circular References
			services.AddMvc()
				.AddJsonOptions(options => {
					options.SerializerSettings.ReferenceLoopHandling =
						Newtonsoft.Json.ReferenceLoopHandling.Ignore;
				});

			// Implement Repositories
			services.AddSingleton<IProductRepository, ProductRepository>();

			// Register the Swagger generator, defining one or more Swagger documents
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new Info
				{
					Title = "Catalog  API", Version = "v1",
					Description = "The Catalog Microservice HTTP API. This is a Data-Driven/CRUD microservice ",
					TermsOfService = "Terms Of Service"
				});
			});

			services.AddCors(options =>
			{
				options.AddPolicy("AllowSpecificOrigin",
					builder => builder.AllowAnyOrigin());
			});

			services.Configure<MvcOptions>(options =>
			{
				options.Filters.Add(new CorsAuthorizationFilterFactory("AllowSpecificOrigin"));
			});
		}

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
