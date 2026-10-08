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

        public Task AdicionarAsync(Funcionario funcionario, CancellationToken cancellationToken = default)
        {
            _context.Funcionarios.Add(funcionario);
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<Funcionario>> ObterTodosAsync(CancellationToken cancellationToken = default) =>
            await _context.Funcionarios.AsNoTracking().ToListAsync(cancellationToken);

        public async Task<Funcionario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            await _context.Funcionarios.FindAsync([id], cancellationToken);

        public Task AtualizarAsync(Funcionario funcionario, CancellationToken cancellationToken = default)
        {
            _context.Funcionarios.Update(funcionario);
            return Task.CompletedTask;
        }

        public async Task<bool> RemoverAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var f = await _context.Funcionarios.FindAsync([id], cancellationToken);
            if (f == null) return false;
            _context.Funcionarios.Remove(f);
            return true;
        }
    }
}
