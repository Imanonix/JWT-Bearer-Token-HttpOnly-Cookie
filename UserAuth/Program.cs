using Application.Services.Interfaces;
using Domain.Interfaces.Repository;
using Infrastructure.Implementation;
using Application.Services.Implementation;
using Application.ModelServices;
using Microsoft.EntityFrameworkCore;
using Infrastructure.AuthDbContext;
using Application.Mapper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IAccountServices, AccountServices>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.Configure<EmailSetting>(builder.Configuration.GetSection("EmailSetting"));
builder.Services.Configure<JWTSetting>(builder.Configuration.GetSection("JWTSetting"));
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();


builder.Services.AddCors(option =>
{
    option.AddPolicy("Policy", builder =>                            
    {
        builder.WithOrigins("https://localhost:3000")                       
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});


builder.Services.AddDbContext<DbAuthDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnectionString"));
});

builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(MapperProfile));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("Policy");
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
