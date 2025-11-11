using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagement.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddedReservationStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "status",
                table: "reservations",
                newName: "reservation_status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "reservation_status",
                table: "reservations",
                newName: "status");
        }
    }
}
