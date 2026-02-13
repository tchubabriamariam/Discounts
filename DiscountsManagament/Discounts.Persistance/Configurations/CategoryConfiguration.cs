// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounts.Persistance.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Description)
                .HasMaxLength(500);

            builder.Property(c => c.IconUrl)
                .HasMaxLength(500);

            builder.HasIndex(c => c.Name)
                .IsUnique();

            builder.HasQueryFilter(c => !c.IsDeleted);
        }
    }
}
