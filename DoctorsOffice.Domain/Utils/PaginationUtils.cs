using DoctorsOffice.Domain.Filters;
using DoctorsOffice.Domain.Wrappers;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Domain.Utils;

public static class PaginationUtils
{
    public static HttpResult<PagedResponse<T>> CreatePagedHttpResult<T>(
        IQueryable<T> recordsQueryable,
        PaginationFilter? paginationFilter)
    {
        var result = new HttpResult<PagedResponse<T>>();
        var totalRecords = recordsQueryable.Count();

        if (paginationFilter is null || (paginationFilter.PageNumber is null && paginationFilter.PageSize is null))
            return result
                .WithValue(new PagedResponse<T>
                {
                    PageNumber = 1,
                    PageSize = totalRecords,
                    TotalRecords = totalRecords,
                    Records = recordsQueryable.ToList()
                });

        switch (paginationFilter.PageNumber)
        {
            case null:
                return result
                    .WithError(new Error { Message = "No page number provided" })
                    .WithStatusCode(StatusCodes.Status400BadRequest);
            case < 1:
                return result
                    .WithError(new Error { Message = "Invalid page number" })
                    .WithStatusCode(StatusCodes.Status400BadRequest);
        }

        switch (paginationFilter.PageSize)
        {
            case null:
                return result
                    .WithError(new Error { Message = "No page size provided" })
                    .WithStatusCode(StatusCodes.Status400BadRequest);
            case < 1:
                return result
                    .WithError(new Error { Message = "Invalid page size" })
                    .WithStatusCode(StatusCodes.Status400BadRequest);
        }

        if (paginationFilter.PageNumber > (int)Math.Ceiling((double)totalRecords / paginationFilter.PageSize.Value))
            return result
                .WithError(new Error { Message = "Range not satisfiable" })
                .WithStatusCode(StatusCodes.Status416RangeNotSatisfiable);

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