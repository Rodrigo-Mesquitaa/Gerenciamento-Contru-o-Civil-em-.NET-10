using CivilConstruction.Application.Interfaces;
using CivilConstruction.Infrastructure.Context;

namespace CivilConstruction.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default) =>
            await _context.SaveChangesAsync(cancellationToken);
    }
}
