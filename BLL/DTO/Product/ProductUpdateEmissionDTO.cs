using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO.Product
{
    public class ProductUpdateEmissionDTO
    {
        public ulong Id { get; set; }
        public decimal CommissionRate { get; set; }
    }
}
