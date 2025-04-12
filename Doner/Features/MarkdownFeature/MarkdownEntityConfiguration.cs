using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Doner.Features.MarkdownFeature;

public class MarkdownEntityConfiguration : IEntityTypeConfiguration<Markdown>
{
    public void Configure(EntityTypeBuilder<Markdown> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Uri).IsRequired().HasMaxLength(200);
        builder.Property(x => x.WorkspaceId).IsRequired();
    }
}