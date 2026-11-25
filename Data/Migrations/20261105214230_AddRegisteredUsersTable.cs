using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StourbridgeFc.Lucky7.Data.Migrations;

/// <inheritdoc />
public partial class _20261105214230_AddRegisteredUsersTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "registered_users",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                name = table.Column<string>(type: "text", nullable: false),
                address_line1 = table.Column<string>(type: "text", nullable: false),
                address_line2 = table.Column<string>(type: "text", nullable: true),
                town = table.Column<string>(type: "text", nullable: false),
                county = table.Column<string>(type: "text", nullable: false),
                postcode = table.Column<string>(type: "text", nullable: false),
                telephone = table.Column<string>(type: "text", nullable: true),
                email = table.Column<string>(type: "text", nullable: false),
                registered_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc' "),
                verification_code = table.Column<string>(type: "text", nullable: true),
                verified_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_registered_users", x => x.id);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "registered_users");
    }
}
