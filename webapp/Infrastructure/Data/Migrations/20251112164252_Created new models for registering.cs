using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TarlBreuJacoBaraKnor.Migrations
{
    /// <inheritdoc />
    public partial class Creatednewmodelsforregistering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "signup_model");

            migrationBuilder.CreateTable(
                name: "register_input_model",
                columns: table => new
                {
                    username = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    email = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    password = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    confirm_password = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    postal_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "register_input_model");

            migrationBuilder.CreateTable(
                name: "signup_model",
                columns: table => new
                {
                    email = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    password = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                });
        }
    }
}
