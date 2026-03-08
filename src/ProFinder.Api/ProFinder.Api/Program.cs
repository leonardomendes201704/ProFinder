using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using ProFinder.Api.Middleware;
using ProFinder.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var problemDetails = new ValidationProblemDetails(context.ModelState)
        {
            Title = "A requisição contém dados inválidos.",
            Status = StatusCodes.Status400BadRequest
        };

        return new BadRequestObjectResult(problemDetails);
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ProFinder API",
        Version = "v1",
        Description = "API REST para captação e gestão de profissionais por profissão e região."
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public partial class Program;
