using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Doner.Features.WorkspaceFeature;

public class WorkspaceInviteConfiguration : IEntityTypeConfiguration<WorkspaceInvite>
{
    public void Configure(EntityTypeBuilder<WorkspaceInvite> builder)
    {
        builder.HasKey(x => new { x.UserId, x.WorkspaceId });
    }
}