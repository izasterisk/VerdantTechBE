using System;
using System.Collections.Generic;
using BLL.DTO.Address;

namespace BLL.DTO.Order
{
    public class OrderResponseDTO
    {
        public ulong Id { get; set; }
        public ulong CustomerId { get; set; }
        public string Status { get; set; } = string.Empty;

        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public string? ShippingMethod { get; set; }
        public string? OrderPaymentMethod { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Notes { get; set; }
        public string? CancelledReason { get; set; }

        public DateTime? CancelledAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public AddressResponseDTO Address { get; set; } = null!;
        public List<OrderDetailResponseDTO> OrderDetails { get; set; } = new();
    }

    public class ProductResponseDTO
    {
        public ulong Id { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Images { get; set; }
        public int WarrantyMonths { get; set; }
        public decimal RatingAverage { get; set; }
    }
}
