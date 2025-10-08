using BLL.DTO.ProductRegistration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IStaffService
    {
        Task<ProductRegistrationReponseDTO> ApproveProductRegistrationAsync(ulong id, ulong staffId, CancellationToken cancellationToken = default);
        Task<ProductRegistrationReponseDTO> RejectProductRegistrationAsync(ulong id, ulong staffId, string reason, CancellationToken cancellationToken = default);
    }
}
