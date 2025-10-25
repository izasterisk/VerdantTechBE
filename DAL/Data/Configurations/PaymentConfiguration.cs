using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using DAL.Data.Models;
using DAL.Data;

namespace DAL.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        
        // Primary Key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();
        
        // Foreign Key to Order
        builder.Property(e => e.OrderId)
            .HasColumnType("bigint unsigned")
            .HasColumnName("order_id")
            .IsRequired();
            
        
        // Enum conversions
        builder.Property(e => e.PaymentMethod)
            .HasConversion(
                v => v.ToString()
                    .ToLowerInvariant()
                    .Replace("creditcard", "credit_card")
                    .Replace("debitcard", "debit_card"),
                v => Enum.Parse<PaymentMethod>(v
                    .Replace("credit_card", "CreditCard")
                    .Replace("debit_card", "DebitCard"), true))
            .HasColumnType("enum('credit_card','debit_card','stripe','cod','payos')")
            .IsRequired()
            .HasColumnName("payment_method");
            
        builder.Property(e => e.PaymentGateway)
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<PaymentGateway>(v, true))
            .HasColumnType("enum('stripe','manual','payos')")
            .IsRequired()
            .HasColumnName("payment_gateway");
            
        builder.Property(e => e.Status)
            .HasConversion(
                v => v.ToString()
                    .ToLowerInvariant()
                    .Replace("partiallyrefunded", "partially_refunded"),
                v => Enum.Parse<PaymentStatus>(v
                    .Replace("partially_refunded", "PartiallyRefunded"), true))
            .HasColumnType("enum('pending','processing','completed','failed','refunded','partially_refunded')")
            .HasDefaultValue(PaymentStatus.Pending);
        
        // Gateway payment ID
        builder.Property(e => e.GatewayPaymentId)
            .HasMaxLength(255)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("gateway_payment_id");
        
        // Amount field
        builder.Property(e => e.Amount)
            .HasPrecision(12, 2)
            .IsRequired();
            
        // Refund fields
        
        // JSON field for gateway response using JsonHelpers
        builder.Property(e => e.GatewayResponse)
            .ConfigureAsJson("gateway_response");
        
        // DateTime fields
        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");
        
        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
            .HasColumnName("updated_at");
        
        // Foreign Key Relationships
        builder.HasOne(d => d.Order)
            .WithMany(p => p.Payments)
            .HasForeignKey(d => d.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
            
        
        // Unique constraint for gateway payment ID
        builder.HasIndex(e => e.GatewayPaymentId)
            .IsUnique()
            .HasDatabaseName("idx_gateway_payment");
        
        // Indexes
        builder.HasIndex(e => e.OrderId)
            .HasDatabaseName("idx_order");
            
        builder.HasIndex(e => e.Status)
            .HasDatabaseName("idx_status");
    }
}
