using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkShortener.Configurations;

public class LinkConfiguration: IEntityTypeConfiguration<Link>
{
    public void Configure(EntityTypeBuilder<Link> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(7);
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.IdempotencyKey).IsUnique();
        builder.Property(x => x.OriginalUrl).IsRequired();
        builder.Property(x => x.ClickCount).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
    }
}