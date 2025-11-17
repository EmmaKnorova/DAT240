using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TarlBreuJacoBaraKnor.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToShoppingCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "shopping_carts",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "user_id",
                table: "shopping_carts");
        }
    }
}
