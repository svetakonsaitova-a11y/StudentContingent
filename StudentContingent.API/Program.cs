using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using StudentContingent.API.Filters;
using StudentContingent.Infrastructure.Data;
using StudentContingent.Infrastructure.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
    {
        // Ошибки доменных правил превращаются в понятные ответы 400/404/409.
        options.Filters.Add<DomainExceptionFilter>();
        // Навигационные свойства вида "Student Student = null!" не должны считаться обязательными во входящем JSON.
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    })
    .AddJsonOptions(options =>
    {
        // Навигации ссылаются друг на друга (группа -> студенты -> группа), циклы пропускаем.
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        // Перечисления в JSON строками: "Studying" вместо 1.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddScoped<StudentRepository>();
builder.Services.AddScoped<StudyGroupRepository>();
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped(typeof(ReferenceRepository<>));

// Клиент Blazor работает на другом адресе, поэтому браузеру нужно разрешение CORS.
var clientOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
    options.AddPolicy("Client", policy => policy
        .WithOrigins(clientOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Client");

app.UseAuthorization();

app.MapControllers();

app.Run();
