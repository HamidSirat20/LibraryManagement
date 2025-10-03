using LibraryManagement.WebAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);


var app = builder.ConfigServices()
                 .ConfigPipeline();

app.Run();
