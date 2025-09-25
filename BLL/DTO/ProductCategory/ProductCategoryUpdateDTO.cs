using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO.ProductCategory
{
    public class ProductCategoryUpdateDTO
    {

        [Required(ErrorMessage = "Tên category không được để trống")]
        [StringLength(255, ErrorMessage = "Tên không được vượt quá 255 ký tự")]
        public string Name { get; set; } = null!;

        [StringLength(255, ErrorMessage = "Mô tả không được vượt quá 255 ký tự")]
        public string? Description { get; set; }

        [StringLength(500, ErrorMessage = "IconUrl không được vượt quá 500 ký tự")]
        public string? IconUrl { get; set; }

        [Required(ErrorMessage = "Slug category không được để trống")]
        [StringLength(255, ErrorMessage = "Tên không được vượt quá 255 ký tự")]
        public string Slug { get; set; } = null!;
        public bool IsActive { get; set; } = true;

    }
}
