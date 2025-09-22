﻿namespace LibraryManagement.WebAPI.Models.Common;
    public class PaginationMetadata
    {
    public int ItemCount { get; set; }
    public int PageCount { get; set; }
    public int PageSize { get; set; }
    public int CurrentPage { get; set; }
    public PaginationMetadata(int itemCount, int currentPage, int pageSize)
    {
        ItemCount = itemCount;
        PageSize = pageSize;
        CurrentPage = currentPage;
        PageCount = (int) Math.Ceiling(ItemCount /(double)PageSize);
    }
    }

