using System;
using LibraryManagement.WebAPI.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LibraryManagement.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddedSaltToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "book_authors",
                keyColumn: "id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"));

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("53128e37-a935-4080-b18f-7daa9cbf2c7c"));

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("fc570d3d-5c68-46b3-89b1-63036519a128"));

            migrationBuilder.DeleteData(
                table: "authors",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "books",
                keyColumn: "id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "publishers",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.AddColumn<byte[]>(
                name: "salt",
                table: "users",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "salt",
                table: "users");

            migrationBuilder.InsertData(
                table: "authors",
                columns: new[] { "id", "bio", "created_at", "email", "first_name", "last_name", "updated_at" },
                values: new object[] { new Guid("44444444-4444-4444-4444-444444444444"), "Author of many great books.", new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc), "charlie.writer@example.com", "Charlie", "Writer", new DateTime(2025, 10, 3, 9, 18, 41, 744, DateTimeKind.Utc).AddTicks(9229) });

            migrationBuilder.InsertData(
                table: "publishers",
                columns: new[] { "id", "address", "created_at", "name", "updated_at", "website" },
                values: new object[] { new Guid("33333333-3333-3333-3333-333333333333"), "789 Publisher Ave, Booktown", new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc), "Tech Books Publishing", new DateTime(2025, 10, 3, 9, 18, 41, 744, DateTimeKind.Utc).AddTicks(8426), "https://techbooks.example.com" });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "address", "avatar_url", "created_at", "email", "first_name", "last_name", "membership_end_date", "membership_start_date", "password", "phone", "role", "updated_at" },
                values: new object[,]
                {
                    { new Guid("53128e37-a935-4080-b18f-7daa9cbf2c7c"), "123 Admin St, Cityville", "https://www.gravatar.com/avatar/admin", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "alice.admin@example.com", "Alice", "Admin", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "hashedpassword", "+1-555-1234", UserRole.Admin, new DateTime(2025, 10, 3, 9, 18, 41, 744, DateTimeKind.Utc).AddTicks(3468) },
                    { new Guid("fc570d3d-5c68-46b3-89b1-63036519a128"), "456 Reader Ln, Townsville", "https://www.gravatar.com/avatar/user", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "bob.reader@example.com", "Bob", "Reader", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "hashedpassword", "+1-555-5678", UserRole.User, new DateTime(2025, 10, 3, 9, 18, 41, 744, DateTimeKind.Utc).AddTicks(4668) }
                });

            migrationBuilder.InsertData(
                table: "books",
                columns: new[] { "id", "cover_image_url", "created_at", "description", "genre", "pages", "published_date", "publisher_id", "title", "updated_at" },
                values: new object[] { new Guid("55555555-5555-5555-5555-555555555555"), "https://example.com/book-cover.png", new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc), "A beginner’s guide to Entity Framework Core with PostgreSQL.", Genre.NonFiction, 350, new DateTime(2024, 12, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("33333333-3333-3333-3333-333333333333"), "Learning EF Core", new DateTime(2025, 10, 3, 9, 18, 41, 745, DateTimeKind.Utc).AddTicks(164) });

            migrationBuilder.InsertData(
                table: "book_authors",
                columns: new[] { "id", "author_id", "book_id", "created_at", "updated_at" },
                values: new object[] { new Guid("66666666-6666-6666-6666-666666666666"), new Guid("44444444-4444-4444-4444-444444444444"), new Guid("55555555-5555-5555-5555-555555555555"), new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 10, 3, 9, 18, 41, 745, DateTimeKind.Utc).AddTicks(1108) });
        }
    }
}
