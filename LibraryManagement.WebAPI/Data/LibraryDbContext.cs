using LibraryManagement.WebAPI.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace LibraryManagement.WebAPI.Data
{
    public class LibraryDbContext : DbContext
    {
        private readonly IConfiguration _config;

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Book> Books { get; set; } = null!;
        public DbSet<Author> Authors { get; set; } = null!;
        public DbSet<Publisher> Publishers { get; set; } = null!;
        public DbSet<Loan> Loans { get; set; } = null!;
        public DbSet<Reservation> Reservations { get; set; } = null!;
        public DbSet<BookAuthor> BookAuthors { get; set; } = null!;
        public DbSet<LateReturnOrLostFee> LateReturnOrLostFees { get; set; } = null!;


        public LibraryDbContext(DbContextOptions<LibraryDbContext> options, IConfiguration config) : base(options)
        {

            _config = config ?? throw new ArgumentNullException(nameof(config));
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var builder = new NpgsqlDataSourceBuilder(_config.GetConnectionString("DefaultConnection"));
            builder.MapEnum<UserRole>();
            builder.MapEnum<Genre>();
            builder.MapEnum<FineStatus>();
            builder.MapEnum<LoanStatus>();
            optionsBuilder.UseSnakeCaseNamingConvention();

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresEnum<UserRole>();
            modelBuilder.HasPostgresEnum<Genre>();
            modelBuilder.HasPostgresEnum<FineStatus>();
            modelBuilder.HasPostgresEnum<LoanStatus>();
 

            base.OnModelCreating(modelBuilder);
        }
    }
}
