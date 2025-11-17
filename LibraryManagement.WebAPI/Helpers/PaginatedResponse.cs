using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.WebAPI.Helpers;
public class PaginatedResponse<T> : List<T>
{
    public int CurrentPage { get; private set; }
    public int PageSize { get; private set; }
    public int TotalRecords { get; private set; }
    public int TotalPages { get; private set; }
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
    public PaginatedResponse(List<T> items, int count, int pageNumber, int pageSize)
    {
        TotalRecords = count;
        PageSize = pageSize;
        CurrentPage = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        AddRange(items);
    }

    public static async Task<PaginatedResponse<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PaginatedResponse<T>(items, count, pageNumber, pageSize);
    }
}
