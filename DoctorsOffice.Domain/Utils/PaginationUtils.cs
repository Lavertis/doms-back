using DoctorsOffice.Domain.Filters;
using DoctorsOffice.Domain.Wrappers;

namespace DoctorsOffice.Domain.Utils;

public static class PaginationUtils
{
    public static CommonResult<PagedResponse<T>> CreatePagedResponse<T>(
        IQueryable<T> recordsQueryable,
        PaginationFilter paginationFilter,
        int totalRecords)
    {
        var result = new CommonResult<PagedResponse<T>>();

        if (paginationFilter.PageNumber is null && paginationFilter.PageSize is null)
            return result.WithValue(new PagedResponse<T>
            {
                PageNumber = 1,
                PageSize = totalRecords,
                TotalRecords = totalRecords,
                Records = recordsQueryable.ToList()
            });
        switch (paginationFilter.PageNumber)
        {
            case null:
                return result.WithError(new Error {Message = "No page number provided"});
            case < 1:
                return result.WithError(new Error {Message = "Invalid page number"});
        }

        switch (paginationFilter.PageSize)
        {
            case null:
                return result.WithError(new Error {Message = "No page size provided"});
            case < 1:
                return result.WithError(new Error {Message = "Invalid page size"});
        }

        if (paginationFilter.PageNumber > (int) Math.Ceiling((double) totalRecords / paginationFilter.PageSize.Value))
            return result.WithError(new Error {Message = "Range not satisfiable"});

        var pageNumber = paginationFilter.PageNumber.Value;
        var pageSize = paginationFilter.PageSize.Value;

        pageSize = Math.Min(pageSize, totalRecords);

        var recordsPaged = recordsQueryable
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return result.WithValue(new PagedResponse<T>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            Records = recordsPaged
        });
    }
}