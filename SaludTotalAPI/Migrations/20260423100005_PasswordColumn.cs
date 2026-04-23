using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaludTotalAPI.Migrations
{
    /// <inheritdoc />
    public partial class PasswordColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ChangePassword",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangePassword",
                table: "AspNetUsers");
        }
    }
}
