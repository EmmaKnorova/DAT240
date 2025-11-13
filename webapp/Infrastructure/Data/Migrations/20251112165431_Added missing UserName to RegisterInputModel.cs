using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TarlBreuJacoBaraKnor.Migrations
{
    /// <inheritdoc />
    public partial class AddedmissingUserNametoRegisterInputModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "username",
                table: "register_input_model",
                newName: "user_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "user_name",
                table: "register_input_model",
                newName: "username");
        }
    }
}
