using CivilConstruction.Domain.Entities;
using CivilConstruction.Domain.Interfaces;
using CivilConstruction.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CivilConstruction.Infrastructure.Repositories
{
    public class CaixaRepository : ICaixaRepository
    {
        private readonly AppDbContext _context;
        public CaixaRepository(AppDbContext context) { _context = context; }

        public async Task InicializarAsync(Caixa caixa)
        {
            await _context.Caixas.AddAsync(caixa);
            await _context.SaveChangesAsync();
        }

        public async Task<Caixa?> ObterAsync() => await _context.Caixas.AsNoTracking().FirstOrDefaultAsync();

        public async Task AtualizarAsync(Caixa caixa)
        {
            _context.Caixas.Update(caixa);
            await _context.SaveChangesAsync();
        }
    }
}
