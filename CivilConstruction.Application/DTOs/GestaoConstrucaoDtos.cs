using System.ComponentModel.DataAnnotations;
using CivilConstruction.Domain.Entities;

namespace CivilConstruction.Application.DTOs;

/// <summary>Dados para cadastrar uma obra.</summary>
public sealed record CriarObraRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public required string Nome { get; init; }
    [Required, StringLength(500, MinimumLength = 1)]
    public required string Endereco { get; init; }
    [Required, StringLength(200, MinimumLength = 1)]
    public required string Cliente { get; init; }
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal Orcamento { get; init; }
    public DateTimeOffset DataInicioPrevista { get; init; }
    public DateTimeOffset? DataFimPrevista { get; init; }
}

/// <summary>Resumo de uma obra.</summary>
public sealed record ObraResponse(
    Guid Id, string Nome, string Endereco, string Cliente, decimal Orcamento,
    DateTimeOffset DataInicioPrevista, DateTimeOffset? DataFimPrevista, StatusObra Status);

/// <summary>Dados para cadastrar um fornecedor.</summary>
public sealed record CriarFornecedorRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public required string Nome { get; init; }
    [Required, StringLength(30, MinimumLength = 1)]
    public required string Documento { get; init; }
    [EmailAddress, StringLength(254)]
    public string Email { get; init; } = string.Empty;
    [StringLength(30)]
    public string Telefone { get; init; } = string.Empty;
}

/// <summary>Resumo de um fornecedor.</summary>
public sealed record FornecedorResponse(Guid Id, string Nome, string Documento, string Email, string Telefone);

/// <summary>Item solicitado em uma ordem de compra.</summary>
public sealed record ItemOrdemCompraRequest
{
    [Required, StringLength(50, MinimumLength = 1)]
    public required string Codigo { get; init; }
    [Required, StringLength(200, MinimumLength = 1)]
    public required string Descricao { get; init; }
    [Range(typeof(decimal), "0.001", "79228162514264337593543950335")]
    public decimal Quantidade { get; init; }
    [Required, StringLength(20, MinimumLength = 1)]
    public required string Unidade { get; init; }
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal ValorUnitario { get; init; }
}

/// <summary>Dados para criar uma ordem de compra vinculada a uma obra e fornecedor.</summary>
public sealed record CriarOrdemCompraRequest
{
    public Guid ObraId { get; init; }
    public Guid FornecedorId { get; init; }
    [Required, MinLength(1)]
    public required IReadOnlyList<ItemOrdemCompraRequest> Itens { get; init; }
}

/// <summary>Ordem de compra e seus itens.</summary>
public sealed record OrdemCompraResponse(
    Guid Id, Guid ObraId, Guid FornecedorId, DateTimeOffset CriadaEm,
    StatusOrdemCompra Status, decimal ValorTotal, IReadOnlyList<ItemOrdemCompraRequest> Itens);

/// <summary>Saldo atual de um insumo no estoque.</summary>
public sealed record ItemEstoqueResponse(
    Guid Id, string Codigo, string Descricao, string Unidade, decimal Quantidade, decimal EstoqueMinimo);

/// <summary>Dados para cadastrar um equipamento ou ativo da empresa.</summary>
public sealed record CriarEquipamentoRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public required string Nome { get; init; }
    [Required, StringLength(50, MinimumLength = 1)]
    public required string NumeroPatrimonio { get; init; }
    [StringLength(100)]
    public string? NumeroSerie { get; init; }
    public Guid? ObraId { get; init; }
    public DateTimeOffset DataAquisicao { get; init; }
    public DateTimeOffset? ProximaManutencao { get; init; }
    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal ValorAquisicao { get; init; }
}

/// <summary>Cadastro e situação de um equipamento.</summary>
public sealed record EquipamentoResponse(
    Guid Id, string Nome, string NumeroPatrimonio, string? NumeroSerie, Guid? ObraId,
    DateTimeOffset DataAquisicao, DateTimeOffset? ProximaManutencao, decimal ValorAquisicao,
    StatusEquipamento Status);

/// <summary>Dados para programar pagamentos recorrentes a um funcionário.</summary>
public sealed record CriarProgramacaoPagamentoRequest
{
    public Guid FuncionarioId { get; init; }
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal ValorBruto { get; init; }
    public PeriodicidadePagamento Periodicidade { get; init; }
    public DateTimeOffset ProximoPagamentoEm { get; init; }
}

/// <summary>Programação recorrente de pagamento.</summary>
public sealed record ProgramacaoPagamentoResponse(
    Guid Id, Guid FuncionarioId, decimal ValorBruto, PeriodicidadePagamento Periodicidade,
    DateTimeOffset ProximoPagamentoEm, bool Ativa);

/// <summary>Pagamento gerado automaticamente a partir de uma programação.</summary>
public sealed record PagamentoFuncionarioResponse(
    Guid Id, Guid ProgramacaoPagamentoId, Guid FuncionarioId, decimal ValorBruto,
    DateTimeOffset Competencia, DateTimeOffset CriadoEm, bool EventoPublicado);

/// <summary>Cadastro de cliente.</summary>
public sealed record CriarClienteRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public required string Nome { get; init; }
    [Required, StringLength(30, MinimumLength = 1)]
    public required string Documento { get; init; }
    [EmailAddress, StringLength(254)]
    public string Email { get; init; } = string.Empty;
    [StringLength(30)]
    public string Telefone { get; init; } = string.Empty;
}

/// <summary>Cliente cadastrado.</summary>
public sealed record ClienteResponse(Guid Id, string Nome, string Documento, string Email, string Telefone);

/// <summary>Cadastro de contrato de execução de obra.</summary>
public sealed record CriarContratoRequest
{
    public Guid ClienteId { get; init; }
    public Guid ObraId { get; init; }
    [Required, StringLength(50, MinimumLength = 1)]
    public required string Numero { get; init; }
    public DateTimeOffset DataAssinatura { get; init; }
    public DateTimeOffset DataInicio { get; init; }
    public DateTimeOffset DataFimPrevista { get; init; }
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal ValorContratado { get; init; }
    [Range(typeof(decimal), "0", "100")]
    public decimal RetencaoPercentual { get; init; }
}

/// <summary>Contrato com o cliente.</summary>
public sealed record ContratoResponse(
    Guid Id, Guid ClienteId, Guid ObraId, string Numero, DateTimeOffset DataAssinatura,
    DateTimeOffset DataInicio, DateTimeOffset DataFimPrevista, decimal ValorContratado,
    decimal ValorMedido, decimal RetencaoPercentual, StatusContrato Status);

/// <summary>Medição física e financeira de contrato.</summary>
public sealed record CriarMedicaoRequest
{
    [Required, StringLength(30, MinimumLength = 1)]
    public required string Referencia { get; init; }
    public DateTimeOffset DataMedicao { get; init; }
    [Range(typeof(decimal), "0.001", "100")]
    public decimal PercentualExecutado { get; init; }
}

/// <summary>Medição de obra registrada no contrato.</summary>
public sealed record MedicaoResponse(
    Guid Id, Guid ContratoId, string Referencia, DateTimeOffset DataMedicao,
    decimal PercentualExecutado, decimal ValorMedido, StatusMedicao Status);

/// <summary>Solicita uma transição de estado da medição.</summary>
public sealed record AlterarStatusMedicaoRequest
{
    [EnumDataType(typeof(StatusMedicao))]
    public StatusMedicao Status { get; init; }
    public DateTimeOffset? Vencimento { get; init; }
}

/// <summary>Lançamento de conta a pagar ou receber.</summary>
public sealed record CriarLancamentoFinanceiroRequest
{
    [Required, StringLength(300, MinimumLength = 1)]
    public required string Descricao { get; init; }
    public TipoLancamentoFinanceiro Tipo { get; init; }
    public Guid? ObraId { get; init; }
    public Guid? FornecedorId { get; init; }
    public Guid? ClienteId { get; init; }
    public Guid? MedicaoContratoId { get; init; }
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal Valor { get; init; }
    public DateTimeOffset Vencimento { get; init; }
    [StringLength(100)]
    public string? ReferenciaExterna { get; init; }
}

/// <summary>Conta a pagar ou receber e sua situação.</summary>
public sealed record LancamentoFinanceiroResponse(
    Guid Id, Guid? ObraId, Guid? FornecedorId, Guid? ClienteId, Guid? MedicaoContratoId,
    string Descricao, TipoLancamentoFinanceiro Tipo, StatusLancamentoFinanceiro Status,
    decimal Valor, DateTimeOffset Vencimento, DateTimeOffset? LiquidadoEm, string? ReferenciaExterna);

/// <summary>Registra pagamento ou recebimento de uma conta em aberto.</summary>
public sealed record LiquidarLancamentoRequest
{
    public DateTimeOffset LiquidadoEm { get; init; }
}

/// <summary>Diário de execução e produtividade diária de uma obra.</summary>
public sealed record CriarApontamentoObraRequest
{
    public Guid ObraId { get; init; }
    public DateTimeOffset Data { get; init; }
    [StringLength(30)]
    public string Clima { get; init; } = string.Empty;
    [Required, StringLength(4000, MinimumLength = 1)]
    public required string Atividades { get; init; }
    [Range(0, 10000)]
    public int TrabalhadoresPresentes { get; init; }
    [Range(typeof(decimal), "0", "24")]
    public decimal HorasTrabalhadas { get; init; }
    [StringLength(4000)]
    public string Observacoes { get; init; } = string.Empty;
}

/// <summary>Registro diário da execução da obra.</summary>
public sealed record ApontamentoObraResponse(
    Guid Id, Guid ObraId, DateTimeOffset Data, string Clima, string Atividades,
    int TrabalhadoresPresentes, decimal HorasTrabalhadas, string Observacoes);

/// <summary>Aloca funcionário a uma obra.</summary>
public sealed record CriarAlocacaoFuncionarioRequest
{
    public Guid ObraId { get; init; }
    public Guid FuncionarioId { get; init; }
    public DateTimeOffset Inicio { get; init; }
    [StringLength(100)]
    public string Funcao { get; init; } = string.Empty;
}

/// <summary>Alocação de funcionário em obra.</summary>
public sealed record AlocacaoFuncionarioResponse(
    Guid Id, Guid ObraId, Guid FuncionarioId, DateTimeOffset Inicio,
    DateTimeOffset? Fim, string Funcao);

/// <summary>Notificação de segurança ou acidente de trabalho.</summary>
public sealed record CriarIncidenteRequest
{
    public Guid ObraId { get; init; }
    public Guid? FuncionarioId { get; init; }
    public DateTimeOffset OcorreuEm { get; init; }
    [EnumDataType(typeof(GravidadeIncidente))]
    public GravidadeIncidente Gravidade { get; init; }
    [Required, StringLength(4000, MinimumLength = 1)]
    public required string Descricao { get; init; }
}

/// <summary>Ocorrência de segurança e ações corretivas.</summary>
public sealed record IncidenteResponse(
    Guid Id, Guid ObraId, Guid? FuncionarioId, DateTimeOffset OcorreuEm,
    GravidadeIncidente Gravidade, string Descricao, string AcoesCorretivas, bool Encerrado);

/// <summary>Atualiza a investigação e o encerramento de incidente.</summary>
public sealed record AtualizarIncidenteRequest
{
    [Required, StringLength(4000, MinimumLength = 1)]
    public required string AcoesCorretivas { get; init; }
    public bool Encerrado { get; init; }
}

/// <summary>Inspeção de qualidade em obra.</summary>
public sealed record CriarInspecaoQualidadeRequest
{
    public Guid ObraId { get; init; }
    [Required, StringLength(50, MinimumLength = 1)]
    public required string Referencia { get; init; }
    [Required, StringLength(100, MinimumLength = 1)]
    public required string Categoria { get; init; }
    public DateTimeOffset InspecionadaEm { get; init; }
    [EnumDataType(typeof(ResultadoInspecao))]
    public ResultadoInspecao Resultado { get; init; }
    [StringLength(4000)]
    public string Observacoes { get; init; } = string.Empty;
}

/// <summary>Resultado da inspeção e estado das ações corretivas.</summary>
public sealed record InspecaoQualidadeResponse(
    Guid Id, Guid ObraId, string Referencia, string Categoria, DateTimeOffset InspecionadaEm,
    ResultadoInspecao Resultado, string Observacoes, string AcaoCorretiva,
    DateTimeOffset? PrazoCorrecao, bool Encerrada);

/// <summary>Ação corretiva para inspeção reprovada ou pendente.</summary>
public sealed record AtualizarAcaoCorretivaRequest
{
    [Required, StringLength(4000, MinimumLength = 1)]
    public required string AcaoCorretiva { get; init; }
    public DateTimeOffset? PrazoCorrecao { get; init; }
    public bool Encerrada { get; init; }
}
