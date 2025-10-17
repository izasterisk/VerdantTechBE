using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO.ProductCategory
{
    public class ProductCategoryCreateDTO
    {
        [Required(ErrorMessage ="Tên category không được để trống")]
        [StringLength(255, ErrorMessage ="Tên không được vượt quá 255 ký tự")]
        public string Name { get; set; } = null!;
        
        [Range(1, ulong.MaxValue, ErrorMessage = "ParentId phải lớn hơn 0")]
        public ulong? ParentId { get; set; }

        //[StringLength(255, ErrorMessage = "Mô tả không được vượt quá 255 ký tự")]
        public string? Description { get; set; }
    }
}
