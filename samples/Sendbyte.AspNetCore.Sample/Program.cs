using Sendbyte.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSendbyte(options =>
{
    options.ApiKey = builder.Configuration["Sendbyte:ApiKey"] ?? "sk_test_placeholder";
});

var app = builder.Build();

app.MapGet("/", () => "Sendbyte ASP.NET Core sample is running");

app.Run();
