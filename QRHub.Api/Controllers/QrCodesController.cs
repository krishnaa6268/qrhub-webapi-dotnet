using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRHub.Application.Common.Models;
using QRHub.Application.DTOs.QrCodes;
using QRHub.Application.Services;
using QRHub.Domain.Enums;

namespace QRHub.Api.Controllers;

[Authorize]
public class QrCodesController : BaseApiController
{
    private readonly IQrCodeService _qrCodeService;

    public QrCodesController(IQrCodeService qrCodeService)
    {
        _qrCodeService = qrCodeService;
    }

    /// <summary>
    /// Create a new customized QR code for the authenticated user
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<QrCodeResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateQrCodeRequest request, CancellationToken cancellationToken)
    {
        var response = await _qrCodeService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, ApiResponse<QrCodeResponse>.Ok(response, "QR code created successfully."));
    }

    /// <summary>
    /// Get paginated list of QR codes created by the authenticated user with optional search and sorting
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<QrCodeResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] QrQueryParameters query, CancellationToken cancellationToken)
    {
        var result = await _qrCodeService.GetUserQrCodesAsync(CurrentUserId, query, cancellationToken);
        return Ok(ApiResponse<PagedResult<QrCodeResponse>>.Ok(result, "QR codes retrieved successfully."));
    }

    /// <summary>
    /// Get details of a specific QR code by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<QrCodeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _qrCodeService.GetByIdAsync(CurrentUserId, id, cancellationToken);
        return Ok(ApiResponse<QrCodeResponse>.Ok(result, "QR code details retrieved successfully."));
    }

    /// <summary>
    /// Download or stream the raw PNG image of a saved QR code
    /// </summary>
    [HttpGet("{id:guid}/image")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetImage([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var (imageBytes, contentType, fileName) = await _qrCodeService.GetQrCodeImageAsync(CurrentUserId, id, cancellationToken);
        return File(imageBytes, contentType, fileName);
    }

    /// <summary>
    /// Update an existing QR code
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<QrCodeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateQrCodeRequest request, CancellationToken cancellationToken)
    {
        var result = await _qrCodeService.UpdateAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(ApiResponse<QrCodeResponse>.Ok(result, "QR code updated successfully."));
    }

    /// <summary>
    /// Delete a QR code
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        await _qrCodeService.DeleteAsync(CurrentUserId, id, cancellationToken);
        return Ok(ApiResponse.Ok("QR code deleted successfully."));
    }

    /// <summary>
    /// Quick generate a QR code image directly without saving (Public endpoint)
    /// </summary>
    [AllowAnonymous]
    [HttpPost("quick-generate")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public IActionResult QuickGeneratePost([FromBody] QuickGenerateQrRequest request)
    {
        var (imageBytes, contentType, fileName) = _qrCodeService.QuickGenerate(request);
        return File(imageBytes, contentType, fileName);
    }

    /// <summary>
    /// Quick generate a QR code image via GET query parameters (Public endpoint, ideal for inline images)
    /// </summary>
    [AllowAnonymous]
    [HttpGet("quick-generate")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public IActionResult QuickGenerateGet(
        [FromQuery] string content,
        [FromQuery] string? foregroundColor = "#000000",
        [FromQuery] string? backgroundColor = "#FFFFFF",
        [FromQuery] QrErrorCorrectionLevel level = QrErrorCorrectionLevel.M,
        [FromQuery] int pixelsPerModule = 20)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return BadRequest(ApiResponse.Fail("The 'content' query parameter is required."));
        }

        var request = new QuickGenerateQrRequest
        {
            Content = content,
            ForegroundColorHex = string.IsNullOrWhiteSpace(foregroundColor) ? "#000000" : foregroundColor,
            BackgroundColorHex = string.IsNullOrWhiteSpace(backgroundColor) ? "#FFFFFF" : backgroundColor,
            ErrorCorrectionLevel = level,
            PixelsPerModule = Math.Clamp(pixelsPerModule, 5, 50)
        };

        var (imageBytes, contentType, fileName) = _qrCodeService.QuickGenerate(request);
        return File(imageBytes, contentType, fileName);
    }
}
