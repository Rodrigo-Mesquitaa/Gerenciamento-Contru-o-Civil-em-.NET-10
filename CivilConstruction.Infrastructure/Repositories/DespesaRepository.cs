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

        public Task AdicionarAsync(Despesa despesa)
        {
            _context.Despesas.Add(despesa);
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<Despesa>> ObterTodasAsync() => await _context.Despesas.AsNoTracking().ToListAsync();

        public async Task<Despesa?> ObterPorIdAsync(Guid id) => await _context.Despesas.FindAsync(id);
    }
}
