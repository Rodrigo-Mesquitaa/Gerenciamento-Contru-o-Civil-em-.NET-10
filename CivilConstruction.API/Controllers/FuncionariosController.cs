using CivilConstruction.API.Services;
using CivilConstruction.Application.DTOs;
using CivilConstruction.Application.Interfaces;
using CivilConstruction.Domain.Events;
using Microsoft.AspNetCore.Mvc;

namespace CivilConstruction.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FuncionariosController : ControllerBase
    {
        private readonly IServicoFuncionario _servico;
        private readonly RabbitMQProducer _producer;
        private readonly ILogger<FuncionariosController> _logger;

        public FuncionariosController(IServicoFuncionario servico, RabbitMQProducer producer, ILogger<FuncionariosController> logger)
        {
            _servico = servico;
            _producer = producer;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get() => Ok(await _servico.ObterTodosAsync());

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var f = await _servico.ObterPorIdAsync(id);
            if (f == null) return NotFound();
            return Ok(f);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] FuncionarioDto dto)
        {
            dto.Id ??= Guid.NewGuid();
            await _servico.CriarAsync(dto);

            var ev = new FuncionarioCadastradoEvent
            {
                Id = dto.Id.Value,
                Nome = dto.Nome,
                ValorDiaria = dto.ValorDiaria,
                DataCadastro = DateTime.UtcNow
            };

            try { _producer.PublicarFuncionarioCadastrado(ev); }
            catch (Exception ex) { _logger.LogError(ex, "Erro publicando evento FuncionarioCadastrado - considere Outbox"); }

            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] FuncionarioDto dto)
        {
            dto.Id = id;
            await _servico.AtualizarAsync(dto);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _servico.RemoverAsync(id);
            return NoContent();
        }
    }
}