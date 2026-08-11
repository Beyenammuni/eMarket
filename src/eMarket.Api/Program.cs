using eMarket.Api.Endpoints.Businesses;
using eMarket.Api.Endpoints.Categories;
using eMarket.Api.Endpoints.Products;
using eMarket.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddIdentityServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapSwagger();
    app.MapSwaggerUI();
}

app.UseHttpsRedirection();
//app.UseAuthentication();
//app.UseAuthorization();
app.MapCategoryEndpoints();
app.MapProductEndpoints();
app.MapBusinessEndpoints();

app.Run();
