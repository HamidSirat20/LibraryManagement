using Asp.Versioning;
using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Services.Implementations;
using LibraryManagement.WebAPI.Services.Interfaces;
using LibraryManagement.WebAPI.Services.ORM;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace LibraryManagement.WebAPI.Extensions
{
    public static class ServiceExtensions
    {
        public static WebApplication ConfigServices(this WebApplicationBuilder webApplication)
        {
            // Add services to the container.

            webApplication.Services.AddControllers( configure =>
            {
                configure.ReturnHttpNotAcceptable = true;
                
            })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                }).AddXmlDataContractSerializerFormatters()
                .ConfigureApiBehaviorOptions(setupAction =>
                {
                    setupAction.InvalidModelStateResponseFactory = context =>
                    {
                        var problemDetails = new ValidationProblemDetails(context.ModelState)
                        {
                            Type = "https://www.librarymanagement.com/modelvalidationproblem",
                            Title = "One or more model validation errors occurred.",
                            Status = StatusCodes.Status422UnprocessableEntity,
                            Detail = "See the errors property for details.",
                            Instance = context.HttpContext.Request.Path
                        };
                        problemDetails.Extensions.Add("traceId", context.HttpContext.TraceIdentifier);
                        return new UnprocessableEntityObjectResult(problemDetails)
                        {
                            ContentTypes = { "application/problem+json" }
                        };
                    };
                });
            //configure support for vendor specific media types
            webApplication.Services.Configure<MvcOptions>(config =>
            {
                var newtonsoftJsonOutputFormatter = config.OutputFormatters
                    .OfType<Microsoft.AspNetCore.Mvc.Formatters.NewtonsoftJsonOutputFormatter>()?
                    .FirstOrDefault();
                if (newtonsoftJsonOutputFormatter != null)
                    {
                    newtonsoftJsonOutputFormatter.SupportedMediaTypes
                        .Add("application/vnd.hamid.hateoas+json");
                }
            });
            // Add services
            webApplication.Services.AddScoped<IBookService, BookService>();
            webApplication.Services.AddScoped<IUserService, UserService>();
            webApplication.Services.AddScoped<IBookCollectionService, BookCollectionService>();
            webApplication.Services.AddScoped<IUserMapper, UserMapper>();
            webApplication.Services.AddScoped<IBookMapper, BookMapper>();
            webApplication.Services.AddScoped<IPropertyCheckerService, PropertyCheckerService>();

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
            //add routing
            webApplication.Services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
                options.AppendTrailingSlash = false;
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
