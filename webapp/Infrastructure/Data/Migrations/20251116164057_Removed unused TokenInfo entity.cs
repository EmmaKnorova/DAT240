using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TarlBreuJacoBaraKnor.Migrations;

/// <inheritdoc />
public partial class RemovedunusedTokenInfoentity : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "token_infos");

        migrationBuilder.DropTable(
            name: "token_model");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "token_infos",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                expired_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                refresh_token = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                username = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_token_infos", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "token_model",
            columns: table => new
            {
                access_token = table.Column<string>(type: "text", nullable: false),
                refresh_token = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
            });
    }
}
