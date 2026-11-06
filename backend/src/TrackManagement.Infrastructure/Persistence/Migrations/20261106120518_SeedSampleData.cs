using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TrackManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedSampleData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Artists",
                columns: new[] { "Id", "Country", "Email", "Name" },
                values: new object[,]
                {
                    { 1, "EG", "nour.elsayed@example.com", "Nour El Sayed" },
                    { 2, "LB", "layla.haddad@example.com", "Layla Haddad" },
                    { 3, "SA", "omar.alrashid@example.com", "Omar Al-Rashid" },
                    { 4, "MA", "yasmine.benali@example.com", "Yasmine Benali" }
                });

            migrationBuilder.InsertData(
                table: "Dsps",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Spotify" },
                    { 2, "Apple Music" },
                    { 3, "YouTube" },
                    { 4, "Anghami" }
                });

            migrationBuilder.InsertData(
                table: "Tracks",
                columns: new[] { "Id", "ArtistId", "Genre", "Isrc", "ReleaseDate", "Status", "Title" },
                values: new object[,]
                {
                    { 1, 1, "Arabic Pop", "EGTKW2400001", new DateOnly(2024, 3, 15), "Distributed", "Leil El Qahera" },
                    { 2, 1, "Mahraganat", "EGTKW2400002", new DateOnly(2024, 9, 1), "Submitted", "Sekket Safar" },
                    { 3, 2, "Indie", "LBTKW2300001", new DateOnly(2023, 11, 20), "Distributed", "Beirut Lights" },
                    { 4, 2, "R&B", "LBTKW2500001", new DateOnly(2025, 2, 14), "Draft", "Ya Habibi Taala" },
                    { 5, 3, "Khaleeji", "SATKW2400001", new DateOnly(2024, 6, 10), "Distributed", "Najd Nights" },
                    { 6, 3, "Hip Hop", "SATKW2500001", new DateOnly(2025, 5, 5), "Submitted", "Riyadh Rhythm" },
                    { 7, 3, "Electronic", "SATKW2600001", new DateOnly(2026, 12, 1), "Draft", "Desert Wind" },
                    { 8, 4, "Rai", "MATKW2400001", new DateOnly(2024, 4, 22), "Distributed", "Casablanca Blue" },
                    { 9, 4, "Electronic", "MATKW2500001", new DateOnly(2025, 8, 30), "Submitted", "Atlas" },
                    { 10, 4, "Arabic Pop", "MATKW2600001", new DateOnly(2026, 10, 1), "Draft", "Medina Dreams" }
                });

            migrationBuilder.InsertData(
                table: "TrackDistributions",
                columns: new[] { "Id", "DspId", "Status", "SubmittedAt", "TrackId" },
                values: new object[,]
                {
                    { 1, 1, "Live", new DateTime(2024, 3, 1, 9, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 2, 2, "Live", new DateTime(2024, 3, 1, 9, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 3, 3, "Live", new DateTime(2024, 3, 1, 9, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 4, 4, "Live", new DateTime(2024, 3, 1, 9, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 5, 1, "Pending", new DateTime(2024, 8, 20, 9, 0, 0, 0, DateTimeKind.Utc), 2 },
                    { 6, 4, "Pending", new DateTime(2024, 8, 20, 9, 0, 0, 0, DateTimeKind.Utc), 2 },
                    { 7, 1, "Live", new DateTime(2023, 11, 5, 9, 0, 0, 0, DateTimeKind.Utc), 3 },
                    { 8, 2, "Rejected", new DateTime(2023, 11, 5, 9, 0, 0, 0, DateTimeKind.Utc), 3 },
                    { 9, 4, "Live", new DateTime(2023, 11, 5, 9, 0, 0, 0, DateTimeKind.Utc), 3 },
                    { 10, 1, "Live", new DateTime(2024, 5, 28, 9, 0, 0, 0, DateTimeKind.Utc), 5 },
                    { 11, 3, "Live", new DateTime(2024, 5, 28, 9, 0, 0, 0, DateTimeKind.Utc), 5 },
                    { 12, 4, "Pending", new DateTime(2024, 5, 28, 9, 0, 0, 0, DateTimeKind.Utc), 5 },
                    { 13, 1, "Rejected", new DateTime(2025, 4, 25, 9, 0, 0, 0, DateTimeKind.Utc), 6 },
                    { 14, 2, "Pending", new DateTime(2025, 4, 25, 9, 0, 0, 0, DateTimeKind.Utc), 6 },
                    { 15, 1, "Live", new DateTime(2024, 4, 10, 9, 0, 0, 0, DateTimeKind.Utc), 8 },
                    { 16, 2, "Live", new DateTime(2024, 4, 10, 9, 0, 0, 0, DateTimeKind.Utc), 8 },
                    { 17, 4, "Live", new DateTime(2024, 4, 10, 9, 0, 0, 0, DateTimeKind.Utc), 8 },
                    { 18, 3, "Pending", new DateTime(2025, 8, 15, 9, 0, 0, 0, DateTimeKind.Utc), 9 },
                    { 19, 4, "Pending", new DateTime(2025, 8, 15, 9, 0, 0, 0, DateTimeKind.Utc), 9 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "TrackDistributions",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Dsps",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Dsps",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Dsps",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Dsps",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Artists",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Artists",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Artists",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Artists",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
