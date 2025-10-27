using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagement.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddImageForBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cover_image_public_id",
                table: "books",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cover_image_public_id",
                table: "books");
        }
    }
}
