namespace BLL.DTO.Dashboard;

public class QueueStatisticsResponseDTO
{
    public int VendorProfile { get; set; }
    public int ProductRegistration { get; set; }
    public int VendorCertificate { get; set; }
    public int ProductCertificate { get; set; }
    public int Request { get; set; }
    public int Total => VendorProfile + ProductRegistration + VendorCertificate + ProductCertificate + Request;
}

