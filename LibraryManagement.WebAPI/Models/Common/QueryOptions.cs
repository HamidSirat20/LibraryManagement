using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.WebAPI.Models.Common;
    public class QueryOptions
    {
    [FromQuery(Name = "search")]
    public string? SearchTerm { get; set; }
    [FromQuery(Name = "page")]
    public int PageNumber { get; set; } = 1;

    [FromQuery(Name = "size")]
    public int PageSize { get; set; } = 20;

    [FromQuery(Name = "sort")]
    public string? SortBy { get; set; } = "Title";

    [FromQuery(Name = "desc")]
    public bool? IsDescending { get; set; } = false;
        
    }

