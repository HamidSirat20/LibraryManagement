using LibraryManagement.WebAPI.Models;
using Microsoft.AspNetCore.Http.HttpResults;
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
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(_config.GetConnectionString("DefaultConnection"));

            dataSourceBuilder.MapEnum<UserRole>("user_role");
            dataSourceBuilder.MapEnum<Genre>("genre");
            dataSourceBuilder.MapEnum<FineStatus>("fine_status");
            dataSourceBuilder.MapEnum<LoanStatus>("loan_status");
            dataSourceBuilder.MapEnum<ReservationStatus>("reservation_status");

            var dataSource = dataSourceBuilder.Build();

            optionsBuilder
                .UseNpgsql(dataSource) 
                .UseSnakeCaseNamingConvention();

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresEnum<UserRole>("public", "user_role");
            modelBuilder.HasPostgresEnum<Genre>("public", "genre");
            modelBuilder.HasPostgresEnum<FineStatus>("public", "fine_status");
            modelBuilder.HasPostgresEnum<LoanStatus>("public", "loan_status");
            modelBuilder.HasPostgresEnum<ReservationStatus>("public", "reservation_status");

            modelBuilder.Entity<User>()
                        .Property(u => u.Role)
                        .HasColumnType("user_role");

            modelBuilder.Entity<Book>()
                        .Property(b => b.Genre)
                        .HasColumnType("genre");

            modelBuilder.Entity<Loan>()
                        .Property(l => l.LoanStatus)
                        .HasColumnType("loan_status");
            
            modelBuilder.Entity<Reservation>()
                        .Property(r => r.ReservationStatus)
                        .HasColumnType("reservation_status");   


            base.OnModelCreating(modelBuilder);
        }
    }
}
