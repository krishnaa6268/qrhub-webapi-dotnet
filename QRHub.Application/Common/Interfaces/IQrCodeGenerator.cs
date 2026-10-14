using QRHub.Domain.Enums;

namespace QRHub.Application.Common.Interfaces;

public interface IQrCodeGenerator
{
    byte[] GeneratePng(
        string content,
        int pixelsPerModule = 20,
        string foregroundColorHex = "#000000",
        string backgroundColorHex = "#FFFFFF",
        QrErrorCorrectionLevel level = QrErrorCorrectionLevel.M);

    string GenerateBase64PngDataUrl(
        string content,
        int pixelsPerModule = 20,
        string foregroundColorHex = "#000000",
        string backgroundColorHex = "#FFFFFF",
        QrErrorCorrectionLevel level = QrErrorCorrectionLevel.M);
}
