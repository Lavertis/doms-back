namespace DoctorsOffice.Domain.Wrappers;

public class PagedResponse<T>
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalRecords { get; init; }
    public IEnumerable<T> Records { get; init; } = default!;
}