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
            
        // Foreign Key to Transaction
        builder.Property(e => e.TransactionId)
            .HasColumnType("bigint unsigned")
            .HasColumnName("transaction_id");
        
        // Enum conversions
        builder.Property(e => e.PaymentMethod)
            .HasConversion(
                v => v.ToString()
                    .ToLowerInvariant()
                    .Replace("creditcard", "credit_card")
                    .Replace("debitcard", "debit_card")
                    .Replace("banktransfer", "bank_transfer"),
                v => Enum.Parse<PaymentMethod>(v
                    .Replace("credit_card", "CreditCard")
                    .Replace("debit_card", "DebitCard")
                    .Replace("bank_transfer", "BankTransfer"), true))
            .HasColumnType("enum('credit_card','debit_card','paypal','stripe','bank_transfer','cod')")
            .IsRequired()
            .HasColumnName("payment_method");
            
        builder.Property(e => e.PaymentGateway)
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<PaymentGateway>(v, true))
            .HasColumnType("enum('stripe','paypal','vnpay','momo','manual')")
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
        
        // Gateway transaction ID
        builder.Property(e => e.GatewayTransactionId)
            .HasMaxLength(255)
            .HasCharSet("utf8mb4")
            .UseCollation("utf8mb4_unicode_ci")
            .HasColumnName("gateway_transaction_id");
        
        // Amount field
        builder.Property(e => e.Amount)
            .HasPrecision(12, 2)
            .IsRequired();
            
        // Refund fields
        builder.Property(e => e.RefundAmount)
            .HasPrecision(12, 2)
            .HasDefaultValue(0.00m)
            .HasColumnName("refund_amount");
            
        builder.Property(e => e.RefundReason)
            .HasMaxLength(500)
            .HasColumnName("refund_reason");
            
        builder.Property(e => e.RefundedAt)
            .HasColumnType("timestamp")
            .HasColumnName("refunded_at");
            
        builder.Property(e => e.PaidAt)
            .HasColumnType("timestamp")
            .HasColumnName("paid_at");
            
        builder.Property(e => e.FailedAt)
            .HasColumnType("timestamp")
            .HasColumnName("failed_at");
        
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
            
        builder.HasOne(d => d.Transaction)
            .WithMany(p => p.Payments)
            .HasForeignKey(d => d.TransactionId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Unique constraint for gateway transaction ID
        builder.HasIndex(e => e.GatewayTransactionId)
            .IsUnique()
            .HasDatabaseName("idx_unique_gateway_transaction");
        
        // Indexes
        builder.HasIndex(e => e.OrderId)
            .HasDatabaseName("idx_order");
            
        builder.HasIndex(e => e.TransactionId)
            .HasDatabaseName("idx_transaction");
            
        builder.HasIndex(e => e.Status)
            .HasDatabaseName("idx_status");
            
        builder.HasIndex(e => e.PaymentMethod)
            .HasDatabaseName("idx_payment_method");
    }
}
