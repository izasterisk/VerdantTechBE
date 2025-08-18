using System;
using System.Collections.Generic;

namespace DAL.Models;

/// <summary>
/// Vendor/seller profile and verification details
/// </summary>
public partial class VendorProfile
{
    public ulong Id { get; set; }

    public ulong UserId { get; set; }

    public string CompanyName { get; set; } = null!;

    public string? BusinessRegistrationNumber { get; set; }

    public string? TaxCode { get; set; }

    public string? CompanyAddress { get; set; }

    /// <summary>
    /// JSON array of sustainability certifications
    /// </summary>
    public string? SustainabilityCredentials { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public ulong? VerifiedBy { get; set; }

    /// <summary>
    /// Encrypted bank details for payments
    /// </summary>
    public string? BankAccountInfo { get; set; }

    /// <summary>
    /// Platform commission percentage
    /// </summary>
    public decimal? CommissionRate { get; set; }

    public decimal? RatingAverage { get; set; }

    public int? TotalReviews { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual User? VerifiedByNavigation { get; set; }
}
