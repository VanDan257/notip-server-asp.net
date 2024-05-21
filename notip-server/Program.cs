using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using notip_server.Data;
using notip_server.Extensions;
using notip_server.Hubs;
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

builder.Services.AddApplicationServices()
    .AddIdentityServices();

builder.Services.AddSignalR();
builder.Services.AddControllers();

builder.Services.AddSingleton<ChatHub>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

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

app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(policy);
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<ChatHub>("/chatHub");
});

app.MapControllers();

app.Run();
