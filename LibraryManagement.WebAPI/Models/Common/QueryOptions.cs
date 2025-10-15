using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.WebAPI.Models.Common;
public class QueryOptions
{
    const int MaxPageSize = 50;
    [FromQuery(Name = "search")]
    public string? SearchTerm { get; set; }
    [FromQuery(Name = "pageNumber")]
    public int PageNumber { get; set; } = 1;
    [FromQuery(Name = "genre")]
    public Genre? Genre { get; set; } = null;

    [FromQuery(Name = "pageSize")]
    private int _pageSize { get; set; } = 20;

    [FromQuery(Name = "sort")]
    public string OrderBy { get; set; } = "Title";

    [FromQuery(Name = "desc")]
    public bool IsDescending { get; set; } = false;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;

    }
    [FromQuery(Name ="fields")]
    public string? Fields { get; set; }
}

