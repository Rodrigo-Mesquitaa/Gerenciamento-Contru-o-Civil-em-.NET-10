using CivilConstruction.Application.DTOs;
using CivilConstruction.Application.Interfaces;
using CivilConstruction.Domain.Entities;
using CivilConstruction.Domain.Interfaces;

namespace CivilConstruction.Application.Services
{
    public class ServicoFuncionario : IServicoFuncionario
    {
        private readonly IFuncionarioRepository _repo;

        public ServicoFuncionario(IFuncionarioRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<FuncionarioDto>> ObterTodosAsync()
        {
            var lista = await _repo.ObterTodosAsync();
            return lista.Select(f => new FuncionarioDto { Id = f.Id, Nome = f.Nome, ValorDiaria = f.ValorDiaria });
        }

        public async Task<FuncionarioDto?> ObterPorIdAsync(Guid id)
        {
            var f = await _repo.ObterPorIdAsync(id);
            if (f == null) return null;
            return new FuncionarioDto { Id = f.Id, Nome = f.Nome, ValorDiaria = f.ValorDiaria };
        }

        public async Task CriarAsync(FuncionarioDto dto)
        {
            var entity = new Funcionario
            {
                Id = dto.Id ?? Guid.NewGuid(),
                Nome = dto.Nome,
                ValorDiaria = dto.ValorDiaria,
                DataCadastro = DateTime.UtcNow,
                Ativo = true
            };
            await _repo.AdicionarAsync(entity);
        }

        public async Task AtualizarAsync(FuncionarioDto dto)
        {
            if (dto.Id == null) throw new ArgumentException("Id é necessário para atualizar.");
            var entity = await _repo.ObterPorIdAsync(dto.Id.Value);
            if (entity == null) throw new InvalidOperationException("Funcionário não encontrado.");
            entity.Nome = dto.Nome;
            entity.ValorDiaria = dto.ValorDiaria;
            await _repo.AtualizarAsync(entity);
        }

        public async Task RemoverAsync(Guid id) => await _repo.RemoverAsync(id);
    }
}
