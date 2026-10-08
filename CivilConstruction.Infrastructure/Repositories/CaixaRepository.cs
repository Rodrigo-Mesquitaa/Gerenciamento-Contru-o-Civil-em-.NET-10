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

        public Task InicializarAsync(Caixa caixa)
        {
            _context.Caixas.Add(caixa);
            return Task.CompletedTask;
        }

        public async Task<Caixa?> ObterAsync() => await _context.Caixas.AsNoTracking().FirstOrDefaultAsync();

        public Task AtualizarAsync(Caixa caixa)
        {
            _context.Caixas.Update(caixa);
            return Task.CompletedTask;
        }
    }
}
