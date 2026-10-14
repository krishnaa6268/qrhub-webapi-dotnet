using QRCoder;
using QRHub.Application.Common.Interfaces;
using QRHub.Domain.Enums;

namespace QRHub.Infrastructure.Services;

public class QrCodeGeneratorService : IQrCodeGenerator
{
    public byte[] GeneratePng(
        string content,
        int pixelsPerModule = 20,
        string foregroundColorHex = "#000000",
        string backgroundColorHex = "#FFFFFF",
        QrErrorCorrectionLevel level = QrErrorCorrectionLevel.M)
    {
        var eccLevel = level switch
        {
            QrErrorCorrectionLevel.L => QRCodeGenerator.ECCLevel.L,
            QrErrorCorrectionLevel.M => QRCodeGenerator.ECCLevel.M,
            QrErrorCorrectionLevel.Q => QRCodeGenerator.ECCLevel.Q,
            QrErrorCorrectionLevel.H => QRCodeGenerator.ECCLevel.H,
            _ => QRCodeGenerator.ECCLevel.M
        };

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(content, eccLevel);
        using var qrCode = new PngByteQRCode(qrCodeData);

        var darkColor = HexToRgbaByteArray(foregroundColorHex, 255);
        var lightColor = HexToRgbaByteArray(backgroundColorHex, 255);

        return qrCode.GetGraphic(pixelsPerModule, darkColor, lightColor);
    }

    public string GenerateBase64PngDataUrl(
        string content,
        int pixelsPerModule = 20,
        string foregroundColorHex = "#000000",
        string backgroundColorHex = "#FFFFFF",
        QrErrorCorrectionLevel level = QrErrorCorrectionLevel.M)
    {
        var bytes = GeneratePng(content, pixelsPerModule, foregroundColorHex, backgroundColorHex, level);
        var base64 = Convert.ToBase64String(bytes);
        return $"data:image/png;base64,{base64}";
    }

    private static byte[] HexToRgbaByteArray(string hex, byte alpha = 255)
    {
        hex = hex.Trim().TrimStart('#');
        if (hex.Length == 6)
        {
            var r = Convert.ToByte(hex.Substring(0, 2), 16);
            var g = Convert.ToByte(hex.Substring(2, 2), 16);
            var b = Convert.ToByte(hex.Substring(4, 2), 16);
            return new byte[] { r, g, b, alpha };
        }

        if (hex.Length == 8)
        {
            var r = Convert.ToByte(hex.Substring(0, 2), 16);
            var g = Convert.ToByte(hex.Substring(2, 2), 16);
            var b = Convert.ToByte(hex.Substring(4, 2), 16);
            var a = Convert.ToByte(hex.Substring(6, 2), 16);
            return new byte[] { r, g, b, a };
        }

        return new byte[] { 0, 0, 0, alpha };
    }
}
