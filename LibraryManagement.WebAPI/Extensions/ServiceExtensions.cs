using Asp.Versioning;
using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Services.Implementations;
using LibraryManagement.WebAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Converters;

namespace LibraryManagement.WebAPI.Extensions
{
    public static class ServiceExtensions
    {
        public static WebApplication ConfigServices(this WebApplicationBuilder webApplication)
        {
            // Add services to the container.

            webApplication.Services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                }).AddXmlDataContractSerializerFormatters();

            // Add services
            webApplication.Services.AddScoped<IBookService, BookService>();
            webApplication.Services.AddScoped<IUserService, UserService>();

            // Add DbContext
            webApplication.Services.AddDbContext<LibraryDbContext>(options =>
            {
                options.UseNpgsql(webApplication.Configuration.GetConnectionString("DefaultConnection"),
                    npgsqlOptions =>
                    {
                        npgsqlOptions.MapEnum<UserRole>("user_role");
                        npgsqlOptions.MapEnum<Genre>("genre");
                        npgsqlOptions.MapEnum<FineStatus>("fine_status");
                        npgsqlOptions.MapEnum<LoanStatus>("loan_status");
                    });
            });

            webApplication.Services.AddEndpointsApiExplorer();
            webApplication.Services.AddSwaggerGen();


            // Add Authentication
            webApplication.Services.AddAuthentication("Bearer")
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = webApplication.Configuration["Authentication:issuer"],
                        ValidAudience = webApplication.Configuration["Authentication:audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.Unicode.GetBytes(webApplication.Configuration["Authentication:secretKey"])),
                    };
                });

            webApplication.Services.AddAuthorization(option =>
            {
                option.AddPolicy("AdminCanAccess", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole("role", "Admin");

                });
            });

            //add versioning
            webApplication.Services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            }).AddMvc();

            return  webApplication.Build();
        }

        public static WebApplication ConfigPipeline(this WebApplication app) 
        {
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }

                app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            return app;
        }
    }
}
