using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogContext
{
    public class CatalogDbContext : DbContext
    {
		public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
		  : base(options)
		{
		}


		//public DbSet<User> Users { get; set; }

		public DbSet<CatalogItem> CatalogItems { get; set; }
		public DbSet<CatalogBrand> CatalogBrands { get; set; }
		public DbSet<CatalogType> CatalogTypes { get; set; }


		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);
			// Customize the ASP.NET Identity model and override the defaults if needed.
			// For example, you can rename the ASP.NET Identity table names and more.
			// Add your customizations after calling base.OnModelCreating(builder);
			//builder.ApplyConfiguration ();

			builder.ApplyConfiguration(new CatalogBrandEntityTypeConfiguration());
			builder.ApplyConfiguration(new CatalogTypeEntityTypeConfiguration());
			builder.ApplyConfiguration(new CatalogItemEntityTypeConfiguration());
		}
	}
}
