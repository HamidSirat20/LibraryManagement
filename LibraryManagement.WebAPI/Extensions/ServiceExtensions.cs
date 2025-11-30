using Asp.Versioning;
using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Helpers;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Services.Implementations;
using LibraryManagement.WebAPI.Services.Interfaces;
using LibraryManagement.WebAPI.Services.ORM;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Converters;
using Serilog;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace LibraryManagement.WebAPI.Extensions;
public static class ServiceExtensions
{
    public static WebApplication ConfigServices(this WebApplicationBuilder webApplication)
    {
        // Add services to the container.

        webApplication.Services.AddControllers(configure =>
        {
            configure.ReturnHttpNotAcceptable = true;
            configure.CacheProfiles.Add("120SecondsCacheProfile",
                new CacheProfile()
                {
                    Duration = 120
                });
            //global authorization filter
            // configure.Filters.Add(new AuthorizeFilter() );
        }
        )
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

        //add logging for error
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                fileSizeLimitBytes: 10 * 1024 * 1024)
            .CreateLogger();

        webApplication.Host.UseSerilog();

        //configure cloudinary settings
        webApplication.Services.Configure<CloudinarySettings>(webApplication.Configuration.GetSection("CloudinarySettings"));
        // Add services
        webApplication.Services.AddScoped<IBooksService, BooksService>();
        webApplication.Services.AddScoped<IUsersService, UsersService>();
        webApplication.Services.AddScoped<IBookCollectionsService, BookCollectionsService>();
        webApplication.Services.AddScoped<IUsersMapper, UsersMapper>();
        webApplication.Services.AddScoped<IBooksMapper, BooksMapper>();
        webApplication.Services.AddScoped<IPropertyCheckerService, PropertyCheckerService>();
        webApplication.Services.AddScoped<IPasswordService, PasswordService>();
        webApplication.Services.AddScoped<IImageService, ImageService>();
        webApplication.Services.AddScoped<IPublishersService, PublishersService>();
        webApplication.Services.AddScoped<IPublishersMapper, PublishersMapper>();
        webApplication.Services.AddScoped<IAuthorsService, AuthorsService>();
        webApplication.Services.AddScoped<IAuthorsMapper, AuthorsMapper>();
        webApplication.Services.AddScoped<ILoansService, LoansService>();
        webApplication.Services.AddScoped<ILoansMapper, LoansMapper>();
        webApplication.Services.AddScoped<ICurrentUserService, CurrentUserService>();
        webApplication.Services.AddScoped<IReservationsMapper, ReservationsMapper>();
        webApplication.Services.AddScoped<IReservationService, ReservationService>();
        webApplication.Services.AddScoped<IEmailsTemplateService, EmailsTemplateService>();
        webApplication.Services.AddScoped<IReservationsQueueService, ReservationsQueueService>();
        webApplication.Services.AddTransient<IEmailService, EmailService>();
        webApplication.Services.AddTransient<ILateReturnOrLostFeeService, LateReturnOrLostFeeService>();
        webApplication.Services.AddTransient<ILateReturnOrLostFeeMapper, LateReturnOrLostFeeMapper>();
        webApplication.Services.AddHttpContextAccessor();

        // Add Background Services
        webApplication.Services.AddHostedService<NotificationBackgroundService>();

        // Add Email service
        webApplication.Services
                            .AddFluentEmail(webApplication.Configuration["Email:Smtp:Username"])
                            .AddSmtpSender(new SmtpClient(webApplication.Configuration["Email:Smtp:Host"])
                            {
                                Port = int.Parse(webApplication.Configuration["Email:Smtp:Port"]),
                                Credentials = new NetworkCredential(
                                    webApplication.Configuration["Email:Smtp:Username"],
                                    webApplication.Configuration["Email:Smtp:Password"]
                                ),
                                EnableSsl = true
                            });

        // Add DbContext
        webApplication.Services.AddDbContext<LibraryDbContext>(options =>
        {
            var connectionString = webApplication.Configuration.GetConnectionString("DefaultConnection");
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MapEnum<UserRole>("user_role");
                npgsqlOptions.MapEnum<Genre>("genre");
                npgsqlOptions.MapEnum<FineStatus>("fine_status");
                npgsqlOptions.MapEnum<LoanStatus>("loan_status");
                npgsqlOptions.MapEnum<ReservationStatus>("reservation_status");
                npgsqlOptions.MapEnum<FineType>("fine_type");
                npgsqlOptions.MapEnum<BookStatus>("book_status");
            });
            options.UseSnakeCaseNamingConvention();
        });

        webApplication.Services.AddResponseCaching();

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
             options.TokenValidationParameters = new TokenValidationParameters
             {
                 ValidateIssuer = true,
                 ValidateAudience = true,
                 ValidateLifetime = true,
                 ValidateIssuerSigningKey = true,
                 ValidIssuer = webApplication.Configuration["Authentication:issuer"],
                 ValidAudience = webApplication.Configuration["Authentication:audience"],
                 IssuerSigningKey = new SymmetricSecurityKey(
                     Encoding.UTF8.GetBytes(webApplication.Configuration["Authentication:secretKey"])
                 ),
                 RoleClaimType = "role"
             };
         });

        webApplication.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminCanAccess", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole("Admin");
            });
        });


        //add versioning
        webApplication.Services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ReportApiVersions = true;
        }).AddMvc();
        return webApplication.Build();
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
        app.UseResponseCaching();
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
}
