using QRHub.Application.Common.Models;
using QRHub.Application.DTOs.QrCodes;

namespace QRHub.Application.Services;

public interface IQrCodeService
{
    Task<QrCodeResponse> CreateAsync(Guid userId, CreateQrCodeRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<QrCodeResponse>> GetUserQrCodesAsync(Guid userId, QrQueryParameters query, CancellationToken cancellationToken = default);
    Task<QrCodeResponse> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
    Task<QrCodeResponse> UpdateAsync(Guid userId, Guid id, UpdateQrCodeRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
    Task<(byte[] ImageBytes, string ContentType, string FileName)> GetQrCodeImageAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
    (byte[] ImageBytes, string ContentType, string FileName) QuickGenerate(QuickGenerateQrRequest request);
}
