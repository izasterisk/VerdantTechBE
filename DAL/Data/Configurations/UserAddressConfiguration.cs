using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;

namespace DAL.Data.Configurations;

public class UserAddressConfiguration : IEntityTypeConfiguration<UserAddress>
{
    public void Configure(EntityTypeBuilder<UserAddress> builder)
    {
        builder.ToTable("user_addresses");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
            
        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .IsRequired();
            
        builder.Property(e => e.AddressId)
            .HasColumnName("address_id")
            .IsRequired();
            
        builder.Property(e => e.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false);
            
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
            
        builder.Property(e => e.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamp");

        // Foreign Key Relationships - One User to Many UserAddresses, One Address to Many UserAddresses
        builder.HasOne(e => e.User)
            .WithMany(u => u.UserAddresses)
            .HasForeignKey(e => e.UserId)
            .HasConstraintName("fk_user_addresses_user_id")
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(e => e.Address)
            .WithMany(a => a.UserAddresses)
            .HasForeignKey(e => e.AddressId)
            .HasConstraintName("fk_user_addresses_address_id")
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        // Composite index cho queries filter theo user và trạng thái
        builder.HasIndex(e => new { e.UserId, e.IsDeleted })
            .HasDatabaseName("idx_user_deleted");
        
        // Composite index cho filter theo address và trạng thái
        builder.HasIndex(e => new { e.AddressId, e.IsDeleted })
            .HasDatabaseName("idx_address_deleted");
    }
}