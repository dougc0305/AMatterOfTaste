using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AMatterOfTaste.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOriginalText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "originaltext",
                table: "recipe",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "originaltext",
                table: "recipe");
        }
    }
}
