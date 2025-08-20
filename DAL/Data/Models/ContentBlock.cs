using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Content block for mixed content in forum posts
/// </summary>
public class ContentBlock
{
    /// <summary>
    /// Order of the content block in the post
    /// </summary>
    public int Order { get; set; }
    
    /// <summary>
    /// Type of content: "text", "image", "video", "link", etc.
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Content data - text, URL, etc.
    /// </summary>
    [Required]
    public string Content { get; set; } = string.Empty;
}
