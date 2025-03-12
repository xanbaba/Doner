using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Doner.Features.SprintFeature;

public class SprintConfiguration: IEntityTypeConfiguration<Sprint>
{
    public void Configure(EntityTypeBuilder<Sprint> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.Name).IsUnique();
        
        builder.Property(e => e.Name).IsRequired();
        builder.Property(e => e.Description).IsRequired();

        builder.Property(e => e.StartDateUtc).IsRequired();
        builder.Property(e => e.DeadlineDateUtc).IsRequired();
        builder.Property(e => e.ExpireDateUtc).IsRequired();
        
        builder.Property(e => e.Status).IsRequired().HasConversion<string>();

    }
}