using Microsoft.EntityFrameworkCore;
using QRHub.Domain.Entities;

namespace QRHub.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<QrCode> QrCodes { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
