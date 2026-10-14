using Microsoft.EntityFrameworkCore;
using QRHub.Application.Common.Exceptions;
using QRHub.Application.Common.Interfaces;
using QRHub.Application.Common.Models;
using QRHub.Application.DTOs.QrCodes;
using QRHub.Domain.Entities;
using System.Text.RegularExpressions;

namespace QRHub.Application.Services;

public class QrCodeService : IQrCodeService
{
    private readonly IApplicationDbContext _context;
    private readonly IQrCodeGenerator _qrCodeGenerator;

    public QrCodeService(
        IApplicationDbContext context,
        IQrCodeGenerator qrCodeGenerator)
    {
        _context = context;
        _qrCodeGenerator = qrCodeGenerator;
    }

    public async Task<QrCodeResponse> CreateAsync(Guid userId, CreateQrCodeRequest request, CancellationToken cancellationToken = default)
    {
        var qrCode = new QrCode
        {
            UserId = userId,
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            ForegroundColorHex = string.IsNullOrWhiteSpace(request.ForegroundColorHex) ? "#000000" : request.ForegroundColorHex.ToUpperInvariant(),
            BackgroundColorHex = string.IsNullOrWhiteSpace(request.BackgroundColorHex) ? "#FFFFFF" : request.BackgroundColorHex.ToUpperInvariant(),
            ErrorCorrectionLevel = request.ErrorCorrectionLevel,
            PixelsPerModule = request.PixelsPerModule,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.QrCodes.Add(qrCode);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponse(qrCode);
    }

    public async Task<PagedResult<QrCodeResponse>> GetUserQrCodesAsync(Guid userId, QrQueryParameters query, CancellationToken cancellationToken = default)
    {
        var dbQuery = _context.QrCodes
            .AsNoTracking()
            .Where(q => q.UserId == userId);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var search = query.SearchTerm.Trim().ToLower();
            dbQuery = dbQuery.Where(q =>
                q.Title.ToLower().Contains(search) ||
                q.Content.ToLower().Contains(search) ||
                (q.Description != null && q.Description.ToLower().Contains(search)));
        }

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        dbQuery = query.SortBy?.ToLowerInvariant() switch
        {
            "title" => query.SortDescending ? dbQuery.OrderByDescending(q => q.Title) : dbQuery.OrderBy(q => q.Title),
            "updatedatutc" => query.SortDescending ? dbQuery.OrderByDescending(q => q.UpdatedAtUtc) : dbQuery.OrderBy(q => q.UpdatedAtUtc),
            _ => query.SortDescending ? dbQuery.OrderByDescending(q => q.CreatedAtUtc) : dbQuery.OrderBy(q => q.CreatedAtUtc)
        };

        var items = await dbQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var responseItems = items.Select(MapToResponse).ToList();

        return new PagedResult<QrCodeResponse>(responseItems, totalCount, query.PageNumber, query.PageSize);
    }

    public async Task<QrCodeResponse> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var qrCode = await _context.QrCodes
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == id && q.UserId == userId, cancellationToken);

        if (qrCode == null)
        {
            throw new NotFoundException($"QR Code with ID '{id}' was not found.");
        }

        return MapToResponse(qrCode);
    }

    public async Task<QrCodeResponse> UpdateAsync(Guid userId, Guid id, UpdateQrCodeRequest request, CancellationToken cancellationToken = default)
    {
        var qrCode = await _context.QrCodes
            .FirstOrDefaultAsync(q => q.Id == id && q.UserId == userId, cancellationToken);

        if (qrCode == null)
        {
            throw new NotFoundException($"QR Code with ID '{id}' was not found.");
        }

        qrCode.Title = request.Title.Trim();
        qrCode.Content = request.Content.Trim();
        qrCode.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        qrCode.ForegroundColorHex = string.IsNullOrWhiteSpace(request.ForegroundColorHex) ? "#000000" : request.ForegroundColorHex.ToUpperInvariant();
        qrCode.BackgroundColorHex = string.IsNullOrWhiteSpace(request.BackgroundColorHex) ? "#FFFFFF" : request.BackgroundColorHex.ToUpperInvariant();
        qrCode.ErrorCorrectionLevel = request.ErrorCorrectionLevel;
        qrCode.PixelsPerModule = request.PixelsPerModule;
        qrCode.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponse(qrCode);
    }

    public async Task DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var qrCode = await _context.QrCodes
            .FirstOrDefaultAsync(q => q.Id == id && q.UserId == userId, cancellationToken);

        if (qrCode == null)
        {
            throw new NotFoundException($"QR Code with ID '{id}' was not found.");
        }

        _context.QrCodes.Remove(qrCode);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<(byte[] ImageBytes, string ContentType, string FileName)> GetQrCodeImageAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var qrCode = await _context.QrCodes
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == id && q.UserId == userId, cancellationToken);

        if (qrCode == null)
        {
            throw new NotFoundException($"QR Code with ID '{id}' was not found.");
        }

        var imageBytes = _qrCodeGenerator.GeneratePng(
            qrCode.Content,
            qrCode.PixelsPerModule,
            qrCode.ForegroundColorHex,
            qrCode.BackgroundColorHex,
            qrCode.ErrorCorrectionLevel);

        var safeFileName = SanitizeFileName(qrCode.Title);
        return (imageBytes, "image/png", $"{safeFileName}-qr.png");
    }

    public (byte[] ImageBytes, string ContentType, string FileName) QuickGenerate(QuickGenerateQrRequest request)
    {
        var imageBytes = _qrCodeGenerator.GeneratePng(
            request.Content,
            request.PixelsPerModule,
            request.ForegroundColorHex,
            request.BackgroundColorHex,
            request.ErrorCorrectionLevel);

        return (imageBytes, "image/png", "qrcode.png");
    }

    private QrCodeResponse MapToResponse(QrCode entity)
    {
        var base64Url = _qrCodeGenerator.GenerateBase64PngDataUrl(
            entity.Content,
            entity.PixelsPerModule,
            entity.ForegroundColorHex,
            entity.BackgroundColorHex,
            entity.ErrorCorrectionLevel);

        return new QrCodeResponse
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Title = entity.Title,
            Content = entity.Content,
            Description = entity.Description,
            ForegroundColorHex = entity.ForegroundColorHex,
            BackgroundColorHex = entity.BackgroundColorHex,
            ErrorCorrectionLevel = entity.ErrorCorrectionLevel.ToString(),
            PixelsPerModule = entity.PixelsPerModule,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc,
            DownloadUrl = $"/api/qrcodes/{entity.Id}/image",
            Base64DataUrl = base64Url
        };
    }

    private static string SanitizeFileName(string name)
    {
        var sanitized = Regex.Replace(name.Trim(), @"[^\w\-\.]+", "_");
        return string.IsNullOrWhiteSpace(sanitized) ? "qrcode" : sanitized.ToLowerInvariant();
    }
}
