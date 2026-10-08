using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CivilConstruction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BusinessOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlocacoesFuncionarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ObraId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FuncionarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Inicio = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Fim = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Funcao = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlocacoesFuncionarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlocacoesFuncionarios_Funcionarios_FuncionarioId",
                        column: x => x.FuncionarioId,
                        principalTable: "Funcionarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlocacoesFuncionarios_Obras_ObraId",
                        column: x => x.ObraId,
                        principalTable: "Obras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ApontamentosObra",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ObraId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Data = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Clima = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Atividades = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    TrabalhadoresPresentes = table.Column<int>(type: "int", nullable: false),
                    HorasTrabalhadas = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Observacoes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApontamentosObra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApontamentosObra_Obras_ObraId",
                        column: x => x.ObraId,
                        principalTable: "Obras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
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
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IncidentesSeguranca",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ObraId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FuncionarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OcorreuEm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Gravidade = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    AcoesCorretivas = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Encerrado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentesSeguranca", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentesSeguranca_Funcionarios_FuncionarioId",
                        column: x => x.FuncionarioId,
                        principalTable: "Funcionarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IncidentesSeguranca_Obras_ObraId",
                        column: x => x.ObraId,
                        principalTable: "Obras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InspecoesQualidade",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ObraId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Referencia = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InspecionadaEm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Resultado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Observacoes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    AcaoCorretiva = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    PrazoCorrecao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Encerrada = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspecoesQualidade", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspecoesQualidade_Obras_ObraId",
                        column: x => x.ObraId,
                        principalTable: "Obras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Contratos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ObraId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DataAssinatura = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DataInicio = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DataFimPrevista = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ValorContratado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RetencaoPercentual = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contratos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contratos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contratos_Obras_ObraId",
                        column: x => x.ObraId,
                        principalTable: "Obras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MedicoesContrato",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContratoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Referencia = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DataMedicao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PercentualExecutado = table.Column<decimal>(type: "decimal(5,3)", precision: 5, scale: 3, nullable: false),
                    ValorMedido = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicoesContrato", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicoesContrato_Contratos_ContratoId",
                        column: x => x.ContratoId,
                        principalTable: "Contratos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LancamentosFinanceiros",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ObraId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FornecedorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MedicaoContratoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Descricao = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Vencimento = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LiquidadoEm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ReferenciaExterna = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LancamentosFinanceiros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LancamentosFinanceiros_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LancamentosFinanceiros_Fornecedores_FornecedorId",
                        column: x => x.FornecedorId,
                        principalTable: "Fornecedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LancamentosFinanceiros_MedicoesContrato_MedicaoContratoId",
                        column: x => x.MedicaoContratoId,
                        principalTable: "MedicoesContrato",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LancamentosFinanceiros_Obras_ObraId",
                        column: x => x.ObraId,
                        principalTable: "Obras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlocacoesFuncionarios_FuncionarioId_Fim",
                table: "AlocacoesFuncionarios",
                columns: new[] { "FuncionarioId", "Fim" });

            migrationBuilder.CreateIndex(
                name: "IX_AlocacoesFuncionarios_ObraId",
                table: "AlocacoesFuncionarios",
                column: "ObraId");

            migrationBuilder.CreateIndex(
                name: "IX_ApontamentosObra_ObraId_Data",
                table: "ApontamentosObra",
                columns: new[] { "ObraId", "Data" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Documento",
                table: "Clientes",
                column: "Documento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_ClienteId",
                table: "Contratos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_Numero",
                table: "Contratos",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_ObraId",
                table: "Contratos",
                column: "ObraId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentesSeguranca_FuncionarioId",
                table: "IncidentesSeguranca",
                column: "FuncionarioId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentesSeguranca_ObraId_Encerrado_OcorreuEm",
                table: "IncidentesSeguranca",
                columns: new[] { "ObraId", "Encerrado", "OcorreuEm" });

            migrationBuilder.CreateIndex(
                name: "IX_InspecoesQualidade_ObraId_Encerrada_InspecionadaEm",
                table: "InspecoesQualidade",
                columns: new[] { "ObraId", "Encerrada", "InspecionadaEm" });

            migrationBuilder.CreateIndex(
                name: "IX_InspecoesQualidade_ObraId_Referencia",
                table: "InspecoesQualidade",
                columns: new[] { "ObraId", "Referencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LancamentosFinanceiros_ClienteId",
                table: "LancamentosFinanceiros",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_LancamentosFinanceiros_FornecedorId",
                table: "LancamentosFinanceiros",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_LancamentosFinanceiros_MedicaoContratoId",
                table: "LancamentosFinanceiros",
                column: "MedicaoContratoId");

            migrationBuilder.CreateIndex(
                name: "IX_LancamentosFinanceiros_ObraId",
                table: "LancamentosFinanceiros",
                column: "ObraId");

            migrationBuilder.CreateIndex(
                name: "IX_LancamentosFinanceiros_Tipo_Status_Vencimento",
                table: "LancamentosFinanceiros",
                columns: new[] { "Tipo", "Status", "Vencimento" });

            migrationBuilder.CreateIndex(
                name: "IX_MedicoesContrato_ContratoId_Referencia",
                table: "MedicoesContrato",
                columns: new[] { "ContratoId", "Referencia" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlocacoesFuncionarios");

            migrationBuilder.DropTable(
                name: "ApontamentosObra");

            migrationBuilder.DropTable(
                name: "IncidentesSeguranca");

            migrationBuilder.DropTable(
                name: "InspecoesQualidade");

            migrationBuilder.DropTable(
                name: "LancamentosFinanceiros");

            migrationBuilder.DropTable(
                name: "MedicoesContrato");

            migrationBuilder.DropTable(
                name: "Contratos");

            migrationBuilder.DropTable(
                name: "Clientes");
        }
    }
}
