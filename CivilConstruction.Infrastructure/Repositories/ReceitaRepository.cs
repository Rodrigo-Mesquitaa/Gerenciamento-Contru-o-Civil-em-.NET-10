using CivilConstruction.Domain.Entities;
using CivilConstruction.Domain.Interfaces;
using CivilConstruction.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CivilConstruction.Infrastructure.Repositories
{
    public class ReceitaRepository : IReceitaRepository
    {
        private readonly AppDbContext _context;
        public ReceitaRepository(AppDbContext context) { _context = context; }

        public Task AdicionarAsync(Receita receita)
        {
            _context.Receitas.Add(receita);
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<Receita>> ObterTodasAsync() => await _context.Receitas.AsNoTracking().ToListAsync();

        public async Task<Receita?> ObterPorIdAsync(Guid id) => await _context.Receitas.FindAsync(id);
    }
}
