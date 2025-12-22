using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Conversations between customer and vendor
/// </summary>
public partial class CustomerVendorConversation
{
    public ulong Id { get; set; }

    public ulong CustomerId { get; set; }

    public ulong VendorId { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? LastMessageAt { get; set; }

    // Navigation Properties
    public virtual User Customer { get; set; } = null!;
    public virtual User Vendor { get; set; } = null!;
    public virtual ICollection<CustomerVendorMessage> CustomerVendorMessages { get; set; } = new List<CustomerVendorMessage>();
}
