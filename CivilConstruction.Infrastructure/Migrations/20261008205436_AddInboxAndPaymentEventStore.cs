using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CivilConstruction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInboxAndPaymentEventStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventStreams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventStreams", x => x.Id);
                    table.CheckConstraint("CK_EventStreams_Version_NonNegative", "[Version] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "DomainEventRecords",
                columns: table => new
                {
                    StreamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SchemaVersion = table.Column<int>(type: "int", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainEventRecords", x => new { x.StreamId, x.Version });
                    table.CheckConstraint("CK_DomainEventRecords_SchemaVersion_Positive", "[SchemaVersion] > 0");
                    table.CheckConstraint("CK_DomainEventRecords_Version_Positive", "[Version] > 0");
                    table.ForeignKey(
                        name: "FK_DomainEventRecords_EventStreams_StreamId",
                        column: x => x.StreamId,
                        principalTable: "EventStreams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DomainEventRecords_EventId",
                table: "DomainEventRecords",
                column: "EventId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DomainEventRecords");

            migrationBuilder.DropTable(
                name: "EventStreams");
        }
    }
}
