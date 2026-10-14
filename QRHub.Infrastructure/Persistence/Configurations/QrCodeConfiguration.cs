using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QRHub.Domain.Entities;

namespace QRHub.Infrastructure.Persistence.Configurations;

public class QrCodeConfiguration : IEntityTypeConfiguration<QrCode>
{
    public void Configure(EntityTypeBuilder<QrCode> builder)
    {
        builder.ToTable("qr_codes");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(q => q.Content)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(q => q.Description)
            .HasMaxLength(500);

        builder.Property(q => q.ForegroundColorHex)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("#000000");

        builder.Property(q => q.BackgroundColorHex)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("#FFFFFF");

        builder.Property(q => q.ErrorCorrectionLevel)
            .HasConversion<string>()
            .HasMaxLength(5);

        builder.Property(q => q.PixelsPerModule)
            .IsRequired()
            .HasDefaultValue(20);

        builder.Property(q => q.CreatedAtUtc)
            .IsRequired();

        // High performance index for listing a user's QR codes ordered by creation date
        builder.HasIndex(q => new { q.UserId, q.CreatedAtUtc });
    }
}
