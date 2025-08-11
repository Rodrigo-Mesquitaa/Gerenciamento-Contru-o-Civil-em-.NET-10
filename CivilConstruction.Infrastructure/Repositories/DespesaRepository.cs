using CivilConstruction.Domain.Entities;
using CivilConstruction.Domain.Interfaces;
using CivilConstruction.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CivilConstruction.Infrastructure.Repositories
{
    public class DespesaRepository : IDespesaRepository
    {
        private readonly AppDbContext _context;
        public DespesaRepository(AppDbContext context) { _context = context; }

        public async Task AdicionarAsync(Despesa despesa)
        {
            await _context.Despesas.AddAsync(despesa);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Despesa>> ObterTodasAsync() => await _context.Despesas.AsNoTracking().ToListAsync();

        public async Task<Despesa?> ObterPorIdAsync(Guid id) => await _context.Despesas.FindAsync(id);
    }
}
