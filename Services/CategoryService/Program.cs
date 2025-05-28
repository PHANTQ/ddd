using CategoriesService.Data;
using CategoriesService.DTO;
using CategoriesService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add 
builder.Services.Configure<ProfanityApiOptions>(builder.Configuration.GetSection("ProfanityApi"));

builder.Services.AddHttpClient<IBadWordService, BadWordService>();



// DI Services
builder.Services.AddScoped<ICategoryService, CategoryService>();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
