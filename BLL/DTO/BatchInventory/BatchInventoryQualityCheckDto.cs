using DAL.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO.BatchInventory
{
    public class BatchInventoryQualityCheckDto
    {
        public QualityCheckStatus Status { get; set; }

        public ulong? QualityCheckedByUserId { get; set; }

        public string? Notes { get; set; }
    }
}
