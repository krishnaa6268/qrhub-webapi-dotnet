namespace QRHub.Application.DTOs.QrCodes;

public class QrCodeResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ForegroundColorHex { get; set; } = "#000000";
    public string BackgroundColorHex { get; set; } = "#FFFFFF";
    public string ErrorCorrectionLevel { get; set; } = "M";
    public int PixelsPerModule { get; set; } = 20;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    // Helpers for client
    public string DownloadUrl { get; set; } = string.Empty;
    public string Base64DataUrl { get; set; } = string.Empty;
}
