using System.ComponentModel.DataAnnotations;
using QRHub.Domain.Enums;

namespace QRHub.Application.DTOs.QrCodes;

public class QuickGenerateQrRequest
{
    [Required(ErrorMessage = "Content to encode is required.")]
    [StringLength(2048, ErrorMessage = "Content cannot exceed 2048 characters.")]
    public string Content { get; set; } = string.Empty;

    [RegularExpression("^#([A-Fa-f0-9]{6})$", ErrorMessage = "ForegroundColorHex must be a valid 6-character hex color (e.g., #000000).")]
    public string ForegroundColorHex { get; set; } = "#000000";

    [RegularExpression("^#([A-Fa-f0-9]{6})$", ErrorMessage = "BackgroundColorHex must be a valid 6-character hex color (e.g., #FFFFFF).")]
    public string BackgroundColorHex { get; set; } = "#FFFFFF";

    public QrErrorCorrectionLevel ErrorCorrectionLevel { get; set; } = QrErrorCorrectionLevel.M;

    [Range(5, 50, ErrorMessage = "PixelsPerModule must be between 5 and 50.")]
    public int PixelsPerModule { get; set; } = 20;
}
