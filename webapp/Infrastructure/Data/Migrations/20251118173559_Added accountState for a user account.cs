using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TarlBreuJacoBaraKnor.Migrations
{
    /// <inheritdoc />
    public partial class AddedaccountStateforauseraccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "approved_by_admin",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "account_state",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "account_state",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<bool>(
                name: "approved_by_admin",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
