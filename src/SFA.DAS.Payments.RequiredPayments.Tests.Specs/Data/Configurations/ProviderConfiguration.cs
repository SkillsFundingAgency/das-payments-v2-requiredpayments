﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace SFA.DAS.Payments.RequiredPayments.Tests.Specs.Data.Configurations
{
    public class ProviderConfiguration : IEntityTypeConfiguration<Models.Provider>
    {
        public void Configure(EntityTypeBuilder<Models.Provider> builder)
        {
            builder.ToTable("TestingProvider", "Payments2");
            builder.HasKey(x => x.Ukprn);
            builder.HasIndex(x => x.LastUsed);
            builder.Property(x => x.LastUsed).HasColumnName("LastUsed");
            builder.Property(x => x.Ukprn).HasColumnName("Ukprn");
        }
    }
}
