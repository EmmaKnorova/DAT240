using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TarlBreuJacoBaraKnor.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItem_ShoppingCart_ShoppingCartId",
                table: "CartItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ShoppingCart",
                table: "ShoppingCart");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FoodItems",
                table: "FoodItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CartItem",
                table: "CartItem");

            migrationBuilder.RenameTable(
                name: "ShoppingCart",
                newName: "shopping_carts");

            migrationBuilder.RenameTable(
                name: "FoodItems",
                newName: "food_items");

            migrationBuilder.RenameTable(
                name: "CartItem",
                newName: "cart_items");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "shopping_carts",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "food_items",
                newName: "price");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "food_items",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "food_items",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "food_items",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "CookTime",
                table: "food_items",
                newName: "cook_time");

            migrationBuilder.RenameColumn(
                name: "Sku",
                table: "cart_items",
                newName: "sku");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "cart_items",
                newName: "price");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "cart_items",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Count",
                table: "cart_items",
                newName: "count");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "cart_items",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ShoppingCartId",
                table: "cart_items",
                newName: "cart_id");

            migrationBuilder.RenameIndex(
                name: "IX_CartItem_ShoppingCartId",
                table: "cart_items",
                newName: "ix_cart_items_cart_id");

            migrationBuilder.AlterColumn<decimal>(
                name: "price",
                table: "food_items",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "food_items",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "food_items",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "price",
                table: "cart_items",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "cart_items",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "pk_shopping_carts",
                table: "shopping_carts",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_food_items",
                table: "food_items",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_cart_items",
                table: "cart_items",
                column: "id");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    password = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    roles = table.Column<int[]>(type: "integer[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_cart_items_shopping_carts_cart_id",
                table: "cart_items",
                column: "cart_id",
                principalTable: "shopping_carts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_cart_items_shopping_carts_cart_id",
                table: "cart_items");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_shopping_carts",
                table: "shopping_carts");

            migrationBuilder.DropPrimaryKey(
                name: "pk_food_items",
                table: "food_items");

            migrationBuilder.DropPrimaryKey(
                name: "pk_cart_items",
                table: "cart_items");

            migrationBuilder.RenameTable(
                name: "shopping_carts",
                newName: "ShoppingCart");

            migrationBuilder.RenameTable(
                name: "food_items",
                newName: "FoodItems");

            migrationBuilder.RenameTable(
                name: "cart_items",
                newName: "CartItem");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "ShoppingCart",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "price",
                table: "FoodItems",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "FoodItems",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "FoodItems",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "FoodItems",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "cook_time",
                table: "FoodItems",
                newName: "CookTime");

            migrationBuilder.RenameColumn(
                name: "sku",
                table: "CartItem",
                newName: "Sku");

            migrationBuilder.RenameColumn(
                name: "price",
                table: "CartItem",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "CartItem",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "count",
                table: "CartItem",
                newName: "Count");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "CartItem",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "cart_id",
                table: "CartItem",
                newName: "ShoppingCartId");

            migrationBuilder.RenameIndex(
                name: "ix_cart_items_cart_id",
                table: "CartItem",
                newName: "IX_CartItem_ShoppingCartId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "FoodItems",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "FoodItems",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "FoodItems",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2048)",
                oldMaxLength: 2048);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "CartItem",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CartItem",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ShoppingCart",
                table: "ShoppingCart",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FoodItems",
                table: "FoodItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CartItem",
                table: "CartItem",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItem_ShoppingCart_ShoppingCartId",
                table: "CartItem",
                column: "ShoppingCartId",
                principalTable: "ShoppingCart",
                principalColumn: "Id");
        }
    }
}
