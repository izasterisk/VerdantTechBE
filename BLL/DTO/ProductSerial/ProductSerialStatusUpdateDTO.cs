using DAL.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO.ProductSerial
{
    public class ProductSerialStatusUpdateDTO
    {
        public ulong Id { get; set; }
        public ProductSerialStatus Status { get; set; }
    }
}
