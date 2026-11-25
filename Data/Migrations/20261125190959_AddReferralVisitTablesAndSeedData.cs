using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StourbridgeFc.Lucky7.Data.Migrations;

/// <inheritdoc />
public partial class _20261125190959_AddReferralVisitTablesAndSeedData : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "referral_visit_sources",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_referral_visit_sources", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "referral_visits",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                source_id = table.Column<int>(type: "integer", nullable: false),
                visit_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_referral_visits", x => x.id);
                table.ForeignKey(
                    name: "fk_referral_visits_referral_visit_sources_source_id",
                    column: x => x.source_id,
                    principalTable: "referral_visit_sources",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_referral_visits_source_id",
            table: "referral_visits",
            column: "source_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "referral_visits");

        migrationBuilder.DropTable(
            name: "referral_visit_sources");
    }
}
