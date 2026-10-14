using QRHub.Domain.Enums;

namespace QRHub.Domain.Entities;

public class QrCode : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Description { get; set; }

    public string ForegroundColorHex { get; set; } = "#000000";
    public string BackgroundColorHex { get; set; } = "#FFFFFF";
    public QrErrorCorrectionLevel ErrorCorrectionLevel { get; set; } = QrErrorCorrectionLevel.M;
    public int PixelsPerModule { get; set; } = 20;
}
