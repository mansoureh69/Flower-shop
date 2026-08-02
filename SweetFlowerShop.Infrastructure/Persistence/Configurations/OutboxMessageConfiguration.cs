using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SweetFlowerShop.Infrastructure.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasMaxLength(1000).IsRequired();
        var payload = builder.Property(x => x.Payload).HasColumnType("jsonb").IsRequired();
        payload.Metadata.SetMaxLength(null);
        builder.Property(x => x.Error).HasMaxLength(4000);
        builder.HasIndex(x => x.ProcessedOnUtc);
    }
}
