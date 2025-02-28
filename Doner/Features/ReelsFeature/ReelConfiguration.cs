using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Doner.Features.ReelsFeature;

public class ReelConfiguration: IEntityTypeConfiguration<Reel>
{
    public void Configure(EntityTypeBuilder<Reel> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Id);
        
        builder.Property(x => x.Name)
            .HasMaxLength(128);
        
        builder.Property(x => x.Description)
            .HasMaxLength(512);
        
        
        builder.HasOne(e => e.Workspace)
            .WithMany()
            .HasForeignKey(e => e.WorkspaceId);  
    }
}