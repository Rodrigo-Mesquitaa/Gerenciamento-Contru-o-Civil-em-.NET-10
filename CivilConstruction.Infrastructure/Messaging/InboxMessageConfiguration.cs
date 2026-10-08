using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CivilConstruction.Infrastructure.Messaging;

internal sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("InboxMessages");
        builder.HasKey(message => new { message.ConsumerName, message.MessageId });
        builder.Property(message => message.ConsumerName).HasMaxLength(200);
        builder.Property(message => message.ProcessedAt).IsRequired();
        builder.HasIndex(message => message.ProcessedAt);
    }
}
