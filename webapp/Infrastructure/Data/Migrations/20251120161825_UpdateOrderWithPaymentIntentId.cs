using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TarlBreuJacoBaraKnor.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderWithPaymentIntentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "courier_id",
                table: "orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_orders_courier_id",
                table: "orders",
                column: "courier_id");

            migrationBuilder.AddForeignKey(
                name: "fk_orders_user_courier_id",
                table: "orders",
                column: "courier_id",
                principalTable: "AspNetUsers",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_orders_user_courier_id",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "ix_orders_courier_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "courier_id",
                table: "orders");
        }
    }
}
