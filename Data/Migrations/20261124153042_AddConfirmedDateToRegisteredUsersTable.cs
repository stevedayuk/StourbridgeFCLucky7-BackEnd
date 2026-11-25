using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StourbridgeFc.Lucky7.Data.Migrations;

/// <inheritdoc />
public partial class _20261124153042_AddConfirmedDateToRegisteredUsersTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "confirmed_date",
            table: "registered_users",
            type: "timestamp with time zone",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "confirmed_date",
            table: "registered_users");
    }
}
