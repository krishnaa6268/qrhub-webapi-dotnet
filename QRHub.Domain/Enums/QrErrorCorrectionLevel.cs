namespace QRHub.Domain.Enums;

public enum QrErrorCorrectionLevel
{
    L, // Low (~7% recovery)
    M, // Medium (~15% recovery)
    Q, // Quartile (~25% recovery)
    H  // High (~30% recovery)
}
