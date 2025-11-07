using BLL.DTO.MediaLink;
using DAL.Data.Models;
using DAL.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Helpers.CertificateFileHelper
{
    public static class CertificateFileHelper
    {
        // Convert DTOs → MediaLink để lưu PDF vào bảng MediaLinks
        public static List<MediaLink> ToMediaLinks(IEnumerable<MediaLinkItemDTO>? src)
        {
            var list = new List<MediaLink>();
            if (src == null) return list;

            var now = DateTime.UtcNow;
            int sort = 0;
            foreach (var i in src)
            {
                list.Add(new MediaLink
                {
                    OwnerType = MediaOwnerType.ProductCertificates, // OwnerId sẽ set trong Repo
                    OwnerId = 0,
                    ImagePublicId = i.ImagePublicId,
                    ImageUrl = i.ImageUrl,
                    Purpose = MediaPurpose.CertificatePdf,
                    SortOrder = ++sort,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
            return list;
        }
    }
}
