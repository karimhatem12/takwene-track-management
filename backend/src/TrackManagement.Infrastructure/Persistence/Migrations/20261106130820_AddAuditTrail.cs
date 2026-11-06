using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditTrail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Tracks",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()"); // backfill for rows that existed before auditing

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Tracks",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "system"); // rows that existed before auditing

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Tracks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Tracks",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TrackDistributions",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()"); // backfill for rows that existed before auditing

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TrackDistributions",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "system"); // rows that existed before auditing

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TrackDistributions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TrackDistributions",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Artists",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()"); // backfill for rows that existed before auditing

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Artists",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "system"); // rows that existed before auditing

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Artists",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Artists",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    UserEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Changes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TraceId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.UpdateData(
                table: "Artists",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.UpdateData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2024, 3, 1, 9, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.UpdateData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2024, 3, 1, 9, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.UpdateData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2024, 3, 1, 9, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.UpdateData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2024, 3, 1, 9, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.UpdateData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2024, 8, 20, 9, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.UpdateData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2024, 8, 20, 9, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.UpdateData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2023, 11, 5, 9, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.UpdateData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2023, 11, 5, 9, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.UpdateData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2023, 11, 5, 9, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.UpdateData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2024, 5, 28, 9, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.UpdateData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2024, 5, 28, 9, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.UpdateData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2024, 5, 28, 9, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.UpdateData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2025, 4, 25, 9, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.UpdateData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2025, 4, 25, 9, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.UpdateData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2024, 4, 10, 9, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.UpdateData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2024, 4, 10, 9, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.UpdateData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2024, 4, 10, 9, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.UpdateData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2025, 8, 15, 9, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.UpdateData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2025, 8, 15, 9, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.UpdateData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.UpdateData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.UpdateData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.UpdateData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.UpdateData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.UpdateData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.UpdateData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.UpdateData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.UpdateData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.UpdateData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityName_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityName", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Timestamp",
                table: "AuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TrackDistributions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TrackDistributions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TrackDistributions");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TrackDistributions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Artists");
        }
    }
}
