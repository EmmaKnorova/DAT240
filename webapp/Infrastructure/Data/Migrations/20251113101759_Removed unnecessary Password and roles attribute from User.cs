using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TarlBreuJacoBaraKnor.Migrations
{
    /// <inheritdoc />
    public partial class RemovedunnecessaryPasswordandrolesattributefromUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "password",
                table: "users");

            migrationBuilder.DropColumn(
                name: "roles",
                table: "users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "password",
                table: "users",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int[]>(
                name: "roles",
                table: "users",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);
        }
    }
}
