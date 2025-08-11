using CivilConstruction.Domain.Entities;
using CivilConstruction.Domain.Interfaces;
using CivilConstruction.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CivilConstruction.Infrastructure.Repositories
{
    public class FuncionarioRepository : IFuncionarioRepository
    {
        private readonly AppDbContext _context;
        public FuncionarioRepository(AppDbContext context) { _context = context; }

        public async Task AdicionarAsync(Funcionario funcionario)
        {
            await _context.Funcionarios.AddAsync(funcionario);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Funcionario>> ObterTodosAsync() => await _context.Funcionarios.AsNoTracking().ToListAsync();

        public async Task<Funcionario?> ObterPorIdAsync(Guid id) => await _context.Funcionarios.FindAsync(id);

        public async Task AtualizarAsync(Funcionario funcionario)
        {
            _context.Funcionarios.Update(funcionario);
            await _context.SaveChangesAsync();
        }

        public async Task RemoverAsync(Guid id)
        {
            var f = await _context.Funcionarios.FindAsync(id);
            if (f == null) return;
            _context.Funcionarios.Remove(f);
            await _context.SaveChangesAsync();
        }
    }
}
