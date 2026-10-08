using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CivilConstruction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnforceSingleActiveEmployeeAllocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AlocacoesFuncionarios_FuncionarioId_Fim",
                table: "AlocacoesFuncionarios");

            migrationBuilder.CreateIndex(
                name: "IX_AlocacoesFuncionarios_FuncionarioId",
                table: "AlocacoesFuncionarios",
                column: "FuncionarioId",
                unique: true,
                filter: "[Fim] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AlocacoesFuncionarios_FuncionarioId",
                table: "AlocacoesFuncionarios");

            migrationBuilder.CreateIndex(
                name: "IX_AlocacoesFuncionarios_FuncionarioId_Fim",
                table: "AlocacoesFuncionarios",
                columns: new[] { "FuncionarioId", "Fim" });
        }
    }
}
