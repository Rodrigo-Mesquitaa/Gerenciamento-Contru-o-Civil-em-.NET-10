using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CivilConstruction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessDataIntegrityChecks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_ProgramacoesPagamento_ValorBruto_Positive",
                table: "ProgramacoesPagamento",
                sql: "[ValorBruto] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PagamentosFuncionarios_ValorBruto_Positive",
                table: "PagamentosFuncionarios",
                sql: "[ValorBruto] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Obras_Orcamento_NonNegative",
                table: "Obras",
                sql: "[Orcamento] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MedicoesContrato_PercentualExecutado_Range",
                table: "MedicoesContrato",
                sql: "[PercentualExecutado] > 0 AND [PercentualExecutado] <= 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MedicoesContrato_ValorMedido_Positive",
                table: "MedicoesContrato",
                sql: "[ValorMedido] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_LancamentosFinanceiros_Valor_Positive",
                table: "LancamentosFinanceiros",
                sql: "[Valor] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ItensOrdemCompra_Quantidade_Positive",
                table: "ItensOrdemCompra",
                sql: "[Quantidade] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ItensOrdemCompra_ValorUnitario_NonNegative",
                table: "ItensOrdemCompra",
                sql: "[ValorUnitario] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ItensEstoque_Quantidades_NonNegative",
                table: "ItensEstoque",
                sql: "[Quantidade] >= 0 AND [EstoqueMinimo] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Funcionarios_ValorDiaria_Positive",
                table: "Funcionarios",
                sql: "[ValorDiaria] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Equipamentos_ValorAquisicao_NonNegative",
                table: "Equipamentos",
                sql: "[ValorAquisicao] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Contratos_RetencaoPercentual_Range",
                table: "Contratos",
                sql: "[RetencaoPercentual] BETWEEN 0 AND 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Contratos_ValorContratado_Positive",
                table: "Contratos",
                sql: "[ValorContratado] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ApontamentosObra_Indicadores_NonNegative",
                table: "ApontamentosObra",
                sql: "[TrabalhadoresPresentes] >= 0 AND [HorasTrabalhadas] >= 0 AND [HorasTrabalhadas] <= 24");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AlocacoesFuncionarios_Periodo_Valid",
                table: "AlocacoesFuncionarios",
                sql: "[Fim] IS NULL OR [Fim] >= [Inicio]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ProgramacoesPagamento_ValorBruto_Positive",
                table: "ProgramacoesPagamento");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PagamentosFuncionarios_ValorBruto_Positive",
                table: "PagamentosFuncionarios");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Obras_Orcamento_NonNegative",
                table: "Obras");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MedicoesContrato_PercentualExecutado_Range",
                table: "MedicoesContrato");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MedicoesContrato_ValorMedido_Positive",
                table: "MedicoesContrato");

            migrationBuilder.DropCheckConstraint(
                name: "CK_LancamentosFinanceiros_Valor_Positive",
                table: "LancamentosFinanceiros");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ItensOrdemCompra_Quantidade_Positive",
                table: "ItensOrdemCompra");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ItensOrdemCompra_ValorUnitario_NonNegative",
                table: "ItensOrdemCompra");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ItensEstoque_Quantidades_NonNegative",
                table: "ItensEstoque");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Funcionarios_ValorDiaria_Positive",
                table: "Funcionarios");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Equipamentos_ValorAquisicao_NonNegative",
                table: "Equipamentos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Contratos_RetencaoPercentual_Range",
                table: "Contratos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Contratos_ValorContratado_Positive",
                table: "Contratos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ApontamentosObra_Indicadores_NonNegative",
                table: "ApontamentosObra");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AlocacoesFuncionarios_Periodo_Valid",
                table: "AlocacoesFuncionarios");
        }
    }
}
