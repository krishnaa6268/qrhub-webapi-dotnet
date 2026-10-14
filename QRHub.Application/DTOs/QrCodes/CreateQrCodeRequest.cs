using System.ComponentModel.DataAnnotations;
using QRHub.Domain.Enums;

namespace QRHub.Application.DTOs.QrCodes;

public class CreateQrCodeRequest
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(150, ErrorMessage = "Title cannot exceed 150 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content to encode into QR code is required.")]
    [StringLength(2048, ErrorMessage = "Content cannot exceed 2048 characters.")]
    public string Content { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }

    [RegularExpression("^#([A-Fa-f0-9]{6})$", ErrorMessage = "ForegroundColorHex must be a valid 6-character hex color (e.g., #000000).")]
    public string ForegroundColorHex { get; set; } = "#000000";

    [RegularExpression("^#([A-Fa-f0-9]{6})$", ErrorMessage = "BackgroundColorHex must be a valid 6-character hex color (e.g., #FFFFFF).")]
    public string BackgroundColorHex { get; set; } = "#FFFFFF";

    public QrErrorCorrectionLevel ErrorCorrectionLevel { get; set; } = QrErrorCorrectionLevel.M;

    [Range(5, 50, ErrorMessage = "PixelsPerModule must be between 5 and 50.")]
    public int PixelsPerModule { get; set; } = 20;
}
