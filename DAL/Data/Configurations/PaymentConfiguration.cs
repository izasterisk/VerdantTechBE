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
        
        // Foreign Key to Transaction (REQUIRED)
        builder.Property(e => e.TransactionId)
            .HasColumnType("bigint unsigned")
            .HasColumnName("transaction_id")
            .IsRequired();
        
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
        builder.HasOne(d => d.Transaction)
            .WithMany(p => p.Payments)
            .HasForeignKey(d => d.TransactionId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(d => d.Order)
            .WithMany(p => p.Payments)
            .HasForeignKey(d => d.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes
        builder.HasIndex(e => e.TransactionId)
            .HasDatabaseName("idx_transaction");
            
        builder.HasIndex(e => e.OrderId)
            .HasDatabaseName("idx_order");
    }
}
        
        // Indexes
        builder.HasIndex(e => e.OrderId)
            .HasDatabaseName("idx_order");
            
        builder.HasIndex(e => e.Status)
            .HasDatabaseName("idx_status");
    }
}
