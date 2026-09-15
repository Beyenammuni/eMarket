namespace eMarket.SharedKernel.Pagination;

public sealed class PaginationRequest
{
    private const int MaxPageSize = 100;

    private int _pageNumber = 1;

    private int _pageSize = 20;

    public int PageNumber
    {
        get => _pageNumber;
        init => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        init => _pageSize =
            value <= 0 ? 20 : Math.Min(value, MaxPageSize);
    }
}