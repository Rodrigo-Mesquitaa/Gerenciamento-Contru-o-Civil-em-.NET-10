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

        public async Task AdicionarAsync(Receita receita)
        {
            await _context.Receitas.AddAsync(receita);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Receita>> ObterTodasAsync() => await _context.Receitas.AsNoTracking().ToListAsync();

        public async Task<Receita?> ObterPorIdAsync(Guid id) => await _context.Receitas.FindAsync(id);
    }
}
