using LibraryManagement.WebAPI.Extensions;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

var app = builder.ConfigServices()
                 .ConfigPipeline();

app.Run();
