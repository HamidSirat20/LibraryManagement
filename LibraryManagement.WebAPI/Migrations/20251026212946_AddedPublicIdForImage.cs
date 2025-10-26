using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagement.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddedPublicIdForImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "public_id",
                table: "users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "public_id",
                table: "users");
        }
    }
}
