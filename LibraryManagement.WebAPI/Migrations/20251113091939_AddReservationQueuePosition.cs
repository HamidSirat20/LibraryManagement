using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagement.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationQueuePosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "queue_position",
                table: "reservations",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "queue_position",
                table: "reservations");
        }
    }
}
