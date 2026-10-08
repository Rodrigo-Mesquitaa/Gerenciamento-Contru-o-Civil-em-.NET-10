using CivilConstruction.Application.DTOs;
using CivilConstruction.Application.Interfaces;
using CivilConstruction.Domain.Entities;
using CivilConstruction.Domain.Events;
using CivilConstruction.Domain.Interfaces;
using System.Text.Json;

namespace CivilConstruction.Application.Services
{
    public class ServicoFuncionario : IServicoFuncionario
    {
        private readonly IFuncionarioRepository _repo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIntegrationEventOutbox _outbox;

        public ServicoFuncionario(
            IFuncionarioRepository repo,
            IUnitOfWork unitOfWork,
            IIntegrationEventOutbox outbox)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
            _outbox = outbox;
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

        public async Task CriarAsync(FuncionarioDto dto, CancellationToken cancellationToken = default)
        {
            var dataCadastro = DateTime.UtcNow;
            var entity = new Funcionario
            {
                Id = dto.Id ?? Guid.NewGuid(),
                Nome = dto.Nome,
                ValorDiaria = dto.ValorDiaria,
                DataCadastro = dataCadastro,
                Ativo = true
            };
            var integrationEvent = new FuncionarioCadastradoEvent
            {
                Id = entity.Id,
                Nome = entity.Nome,
                ValorDiaria = entity.ValorDiaria,
                DataCadastro = dataCadastro
            };

            await _repo.AdicionarAsync(entity);
            _outbox.Add(
                integrationEvent.Id,
                IntegrationEventTypes.EmployeeRegistered,
                1,
                JsonSerializer.Serialize(integrationEvent),
                new DateTimeOffset(dataCadastro, TimeSpan.Zero));
            await _unitOfWork.CommitAsync(cancellationToken);
        }

        public async Task<bool> AtualizarAsync(FuncionarioDto dto)
        {
            if (dto.Id == null) return false;
            var entity = await _repo.ObterPorIdAsync(dto.Id.Value);
            if (entity == null) return false;
            entity.Nome = dto.Nome;
            entity.ValorDiaria = dto.ValorDiaria;
            await _repo.AtualizarAsync(entity);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            var removed = await _repo.RemoverAsync(id);
            if (!removed) return false;

            await _unitOfWork.CommitAsync();
            return true;
        }
    }
}
