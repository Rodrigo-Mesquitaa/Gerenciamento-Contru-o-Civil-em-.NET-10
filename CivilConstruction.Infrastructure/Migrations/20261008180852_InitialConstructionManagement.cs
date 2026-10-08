using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CivilConstruction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialConstructionManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Caixas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaldoInicial = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SaldoAtual = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Caixas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Despesas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Despesas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fornecedores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Documento = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    Telefone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fornecedores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Funcionarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ValorDiaria = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Funcionarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItensEstoque",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Unidade = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Quantidade = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    EstoqueMinimo = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensEstoque", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Obras",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Endereco = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Cliente = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Orcamento = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DataInicioPrevista = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DataFimPrevista = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Obras", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Receitas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receitas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProgramacoesPagamento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FuncionarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ValorBruto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Periodicidade = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ProximoPagamentoEm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Ativa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramacoesPagamento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramacoesPagamento_Funcionarios_FuncionarioId",
                        column: x => x.FuncionarioId,
                        principalTable: "Funcionarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Equipamentos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NumeroPatrimonio = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NumeroSerie = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ObraId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DataAquisicao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ProximaManutencao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ValorAquisicao = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipamentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Equipamentos_Obras_ObraId",
                        column: x => x.ObraId,
                        principalTable: "Obras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "OrdensCompra",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ObraId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FornecedorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CriadaEm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdensCompra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdensCompra_Fornecedores_FornecedorId",
                        column: x => x.FornecedorId,
                        principalTable: "Fornecedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdensCompra_Obras_ObraId",
                        column: x => x.ObraId,
                        principalTable: "Obras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PagamentosFuncionarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProgramacaoPagamentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FuncionarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ValorBruto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Competencia = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EventoPublicado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagamentosFuncionarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PagamentosFuncionarios_Funcionarios_FuncionarioId",
                        column: x => x.FuncionarioId,
                        principalTable: "Funcionarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PagamentosFuncionarios_ProgramacoesPagamento_ProgramacaoPagamentoId",
                        column: x => x.ProgramacaoPagamentoId,
                        principalTable: "ProgramacoesPagamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItensOrdemCompra",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrdemCompraId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Quantidade = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Unidade = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ValorUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensOrdemCompra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItensOrdemCompra_OrdensCompra_OrdemCompraId",
                        column: x => x.OrdemCompraId,
                        principalTable: "OrdensCompra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Equipamentos_NumeroPatrimonio",
                table: "Equipamentos",
                column: "NumeroPatrimonio",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Equipamentos_ObraId",
                table: "Equipamentos",
                column: "ObraId");

            migrationBuilder.CreateIndex(
                name: "IX_Fornecedores_Documento",
                table: "Fornecedores",
                column: "Documento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItensEstoque_Codigo",
                table: "ItensEstoque",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItensOrdemCompra_OrdemCompraId",
                table: "ItensOrdemCompra",
                column: "OrdemCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdensCompra_FornecedorId",
                table: "OrdensCompra",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdensCompra_ObraId",
                table: "OrdensCompra",
                column: "ObraId");

            migrationBuilder.CreateIndex(
                name: "IX_PagamentosFuncionarios_FuncionarioId",
                table: "PagamentosFuncionarios",
                column: "FuncionarioId");

            migrationBuilder.CreateIndex(
                name: "IX_PagamentosFuncionarios_ProgramacaoPagamentoId_Competencia",
                table: "PagamentosFuncionarios",
                columns: new[] { "ProgramacaoPagamentoId", "Competencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgramacoesPagamento_Ativa_ProximoPagamentoEm",
                table: "ProgramacoesPagamento",
                columns: new[] { "Ativa", "ProximoPagamentoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_ProgramacoesPagamento_FuncionarioId",
                table: "ProgramacoesPagamento",
                column: "FuncionarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Caixas");

            migrationBuilder.DropTable(
                name: "Despesas");

            migrationBuilder.DropTable(
                name: "Equipamentos");

            migrationBuilder.DropTable(
                name: "ItensEstoque");

            migrationBuilder.DropTable(
                name: "ItensOrdemCompra");

            migrationBuilder.DropTable(
                name: "PagamentosFuncionarios");

            migrationBuilder.DropTable(
                name: "Receitas");

            migrationBuilder.DropTable(
                name: "OrdensCompra");

            migrationBuilder.DropTable(
                name: "ProgramacoesPagamento");

            migrationBuilder.DropTable(
                name: "Fornecedores");

            migrationBuilder.DropTable(
                name: "Obras");

            migrationBuilder.DropTable(
                name: "Funcionarios");
        }
    }
}
