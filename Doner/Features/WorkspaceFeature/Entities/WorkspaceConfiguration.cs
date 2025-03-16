using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Doner.Features.WorkspaceFeature.Entities;

public class WorkspaceConfiguration: IEntityTypeConfiguration<Workspace>
{
    public void Configure(EntityTypeBuilder<Workspace> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => c.Name);

        builder.Property(c => c.Name)
            .IsRequired().HasMaxLength(100);
        
        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.HasOne(e => e.Owner)
            .WithMany()
            .HasForeignKey(e => e.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Invitees)
            .WithOne(e => e.Workspace)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
