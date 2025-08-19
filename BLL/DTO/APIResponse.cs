using System.Net;

namespace BLL.DTO;

public class APIResponse
{
    public bool Status { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public dynamic Data { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
}

public class PagedResponse<T>
{
    public List<T> Data { get; set; } = new List<T>();
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}