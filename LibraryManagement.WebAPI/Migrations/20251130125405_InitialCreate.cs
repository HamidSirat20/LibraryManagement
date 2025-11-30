using System;
using LibraryManagement.WebAPI.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagement.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:book_status", "available,damaged,lost,removed,under_repair")
                .Annotation("Npgsql:Enum:fine_status", "cancelled,notified,paid,pending,waived")
                .Annotation("Npgsql:Enum:fine_type", "late_return,lost_item")
                .Annotation("Npgsql:Enum:genre", "biography,fantasy,fiction,history,horror,mystery,non_fiction,other,romance,science_fiction,thriller")
                .Annotation("Npgsql:Enum:loan_status", "active,lost,overdue,pending,returned")
                .Annotation("Npgsql:Enum:reservation_status", "cancelled,fulfilled,notified,pending")
                .Annotation("Npgsql:Enum:user_role", "admin,user");

            migrationBuilder.CreateTable(
                name: "authors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    bio = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_authors", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "publishers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    website = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_publishers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    phone = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    salt = table.Column<byte[]>(type: "bytea", nullable: false),
                    avatar_url = table.Column<string>(type: "text", nullable: false),
                    public_id = table.Column<string>(type: "text", nullable: true),
                    role = table.Column<UserRole>(type: "user_role", nullable: false),
                    membership_start_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    membership_end_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "books",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    cover_image_url = table.Column<string>(type: "text", nullable: false),
                    cover_image_public_id = table.Column<string>(type: "text", nullable: true),
                    published_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    genre = table.Column<Genre>(type: "genre", nullable: false),
                    book_status = table.Column<BookStatus>(type: "book_status", nullable: false),
                    pages = table.Column<int>(type: "integer", nullable: false),
                    publisher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_books", x => x.id);
                    table.ForeignKey(
                        name: "fk_books_publishers_publisher_id",
                        column: x => x.publisher_id,
                        principalTable: "publishers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "book_authors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
                    book_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_book_authors", x => x.id);
                    table.ForeignKey(
                        name: "fk_book_authors_authors_author_id",
                        column: x => x.author_id,
                        principalTable: "authors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_book_authors_books_book_id",
                        column: x => x.book_id,
                        principalTable: "books",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "loans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    book_id = table.Column<Guid>(type: "uuid", nullable: false),
                    loan_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    due_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    return_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    loan_status = table.Column<LoanStatus>(type: "loan_status", nullable: false),
                    late_fee = table.Column<decimal>(type: "numeric", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_loans", x => x.id);
                    table.ForeignKey(
                        name: "fk_loans_books_book_id",
                        column: x => x.book_id,
                        principalTable: "books",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_loans_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reservations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reserved_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    pickup_deadline = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    reservation_status = table.Column<ReservationStatus>(type: "reservation_status", nullable: false),
                    queue_position = table.Column<int>(type: "integer", nullable: false),
                    book_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reservations", x => x.id);
                    table.ForeignKey(
                        name: "fk_reservations_books_book_id",
                        column: x => x.book_id,
                        principalTable: "books",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_reservations_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "late_return_or_lost_fees",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    issued_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    paid_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    status = table.Column<FineStatus>(type: "fine_status", nullable: false),
                    fine_type = table.Column<FineType>(type: "fine_type", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    loan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_late_return_or_lost_fees", x => x.id);
                    table.ForeignKey(
                        name: "fk_late_return_or_lost_fees_loans_loan_id",
                        column: x => x.loan_id,
                        principalTable: "loans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_late_return_or_lost_fees_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_book_authors_author_id",
                table: "book_authors",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "ix_book_authors_book_id",
                table: "book_authors",
                column: "book_id");

            migrationBuilder.CreateIndex(
                name: "ix_books_publisher_id",
                table: "books",
                column: "publisher_id");

            migrationBuilder.CreateIndex(
                name: "ix_late_return_or_lost_fees_loan_id",
                table: "late_return_or_lost_fees",
                column: "loan_id");

            migrationBuilder.CreateIndex(
                name: "ix_late_return_or_lost_fees_user_id",
                table: "late_return_or_lost_fees",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_loans_book_id",
                table: "loans",
                column: "book_id");

            migrationBuilder.CreateIndex(
                name: "ix_loans_user_id",
                table: "loans",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_reservations_book_id",
                table: "reservations",
                column: "book_id");

            migrationBuilder.CreateIndex(
                name: "ix_reservations_user_id",
                table: "reservations",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "book_authors");

            migrationBuilder.DropTable(
                name: "late_return_or_lost_fees");

            migrationBuilder.DropTable(
                name: "reservations");

            migrationBuilder.DropTable(
                name: "authors");

            migrationBuilder.DropTable(
                name: "loans");

            migrationBuilder.DropTable(
                name: "books");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "publishers");
        }
    }
}
