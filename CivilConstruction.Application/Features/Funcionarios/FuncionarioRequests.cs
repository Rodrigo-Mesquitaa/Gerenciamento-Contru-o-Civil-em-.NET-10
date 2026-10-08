using MediatR;
using System.ComponentModel.DataAnnotations;

namespace CivilConstruction.Application.Features.Funcionarios;

/// <summary>Dados de cadastro de um funcionário.</summary>
public sealed record CriarFuncionarioRequest : IRequest<FuncionarioResponse>
{
    /// <summary>Identificador opcional solicitado pelo cliente.</summary>
    public Guid? Id { get; init; }

    /// <summary>Nome completo do funcionário.</summary>
    [Required, StringLength(200, MinimumLength = 1)]
    public required string Nome { get; init; }

    /// <summary>Valor bruto da diária contratada.</summary>
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal ValorDiaria { get; init; }
}

/// <summary>Dados para atualizar nome e diária de um funcionário.</summary>
public sealed record AtualizarFuncionarioRequest
{
    /// <summary>Nome completo do funcionário.</summary>
    [Required, StringLength(200, MinimumLength = 1)]
    public required string Nome { get; init; }

    /// <summary>Valor bruto da diária contratada.</summary>
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal ValorDiaria { get; init; }
}

/// <summary>Representação pública resumida de um funcionário.</summary>
public sealed record FuncionarioResponse(Guid Id, string Nome, decimal ValorDiaria);

/// <summary>Consulta todos os funcionários.</summary>
public sealed record ListarFuncionariosQuery : IRequest<IReadOnlyList<FuncionarioResponse>>;

/// <summary>Consulta um funcionário pelo identificador.</summary>
public sealed record ObterFuncionarioQuery(Guid Id) : IRequest<FuncionarioResponse?>;

/// <summary>Atualiza os dados cadastrais de um funcionário.</summary>
public sealed record AtualizarFuncionarioCommand(
    Guid Id,
    string Nome,
    decimal ValorDiaria) : IRequest<bool>;

/// <summary>Remove um funcionário pelo identificador.</summary>
public sealed record RemoverFuncionarioCommand(Guid Id) : IRequest<bool>;
