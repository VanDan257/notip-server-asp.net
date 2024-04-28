using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using notip_server.Data;
using notip_server.Extensions;
using notip_server.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
EnviConfig.Config(builder.Configuration);
var policy = "_anyCorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(policy,
        builder =>
        {
            builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
        });
});

builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddIdentityServices();

#region EntityFramework Core
builder.Services.AddDbContext<DbChatContext>(option =>
{
    option.UseLazyLoadingProxies().UseMySql(EnviConfig.DbConnectionString, ServerVersion.AutoDetect(EnviConfig.DbConnectionString));
});
#endregion


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
