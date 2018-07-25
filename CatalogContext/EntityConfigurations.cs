using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogContext
{
	class CatalogBrandEntityTypeConfiguration
		 : IEntityTypeConfiguration<CatalogBrand>
	{
		public void Configure(EntityTypeBuilder<CatalogBrand> builder)
		{
			builder.ToTable("CatalogBrand");

			builder.HasKey(ci => ci.Id);

			builder.Property(ci => ci.Id)
			   .ForSqlServerUseSequenceHiLo("catalog_brand_hilo")
			   .IsRequired();

			builder.Property(cb => cb.Brand)
				.IsRequired()
				.HasMaxLength(100);
		}
	}

	class CatalogItemEntityTypeConfiguration
	   : IEntityTypeConfiguration<CatalogItem>
	{
		public void Configure(EntityTypeBuilder<CatalogItem> builder)
		{
			builder.ToTable("Catalog");

			builder.Property(ci => ci.Id)
				.ForSqlServerUseSequenceHiLo("catalog_hilo")
				.IsRequired();

			builder.Property(ci => ci.Name)
				.IsRequired(true)
				.HasMaxLength(50);

			builder.Property(ci => ci.Price)
				.IsRequired(true);

			builder.Property(ci => ci.PictureFileName)
				.IsRequired(false);

			builder.Ignore(ci => ci.PictureUri);

			builder.HasOne(ci => ci.CatalogBrand)
				.WithMany()
				.HasForeignKey(ci => ci.CatalogBrandId);

			builder.HasOne(ci => ci.CatalogType)
				.WithMany()
				.HasForeignKey(ci => ci.CatalogTypeId);
		}
	}

	class CatalogTypeEntityTypeConfiguration
	   : IEntityTypeConfiguration<CatalogType>
	{
		public void Configure(EntityTypeBuilder<CatalogType> builder)
		{
			builder.ToTable("CatalogType");

			builder.HasKey(ci => ci.Id);

			builder.Property(ci => ci.Id)
			   .ForSqlServerUseSequenceHiLo("catalog_type_hilo")
			   .IsRequired();

			builder.Property(cb => cb.Type)
				.IsRequired()
				.HasMaxLength(100);
		}
	}
}
