namespace QRHub.Application.DTOs.QrCodes;

public class QrQueryParameters
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;
    private int _pageNumber = 1;

    public string? SearchTerm { get; set; }

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : (value < 1 ? 10 : value);
    }

    public string? SortBy { get; set; } = "CreatedAtUtc";
    public bool SortDescending { get; set; } = true;
}
