using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Doner.Features.AuthFeature.Entities;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Id
        builder.HasKey(x => x.Id);
        
        // FirstName
        builder.Property(x => x.FirstName).IsRequired().HasMaxLength(50).IsUnicode();
        builder.HasIndex(x => x.FirstName).IsUnique(false);
        
        // LastName
        builder.Property(x => x.LastName).HasMaxLength(100).IsUnicode();
        
        // MiddleName
        builder.Property(x => x.MiddleName).HasMaxLength(50).IsUnicode();
        
        // Login
        builder.Property(x => x.Login).IsRequired().HasMaxLength(100).IsUnicode(false);
        builder.HasIndex(x => x.Login).IsUnique();
        
        // PasswordHash
        builder.Property(x => x.PasswordHash).IsRequired().HasMaxLength(64);
        
        // PasswordSalt
        builder.Property(x => x.PasswordSalt).IsRequired().HasMaxLength(32);
        
        // Email
        builder.Property(x => x.Email).HasMaxLength(255).IsUnicode();
        builder.HasIndex(x => x.Email).IsUnique();
        
        // InvitedWorkspaces
        builder.HasMany(x => x.InvitedWorkspaces)
            .WithOne(i => i.User)
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}