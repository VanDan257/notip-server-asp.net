using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Microsoft.EntityFrameworkCore;
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
    .AddIdentityServices()
    .AddAwsS3Services(builder.Configuration);

builder.Services.AddSignalR();
builder.Services.AddControllers();

builder.Services.AddSingleton<ChatHub>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

#region EntityFramework Core
builder.Services.AddDbContext<DbChatContext>(option =>
{
    option.UseMySql(EnviConfig.DbConnectionString, ServerVersion.AutoDetect(EnviConfig.DbConnectionString))
    .LogTo(Console.WriteLine, LogLevel.Information);
});
#endregion

#region Config Amazon S3

builder.Services.AddAWSService<IAmazonS3>(new AWSOptions
{
    // DefaultClientConfig = new AmazonS3Config
    // {
    //     ServiceURL = EnviConfig.ServiceURLAwsS3
    // },
    Credentials = new Amazon.Runtime.BasicAWSCredentials(
        EnviConfig.AccessKeyAwsS3,
        EnviConfig.SecretKeyAwsS3
    )
});

// var s3Config = new AmazonS3Config
// {
//    ServiceURL = EnviConfig.ServiceURLAwsS3
// };

// var awsOptions = new AWSOptions
// {
//    Credentials = new Amazon.Runtime.BasicAWSCredentials(
//        EnviConfig.AccessKeyAwsS3,
//        EnviConfig.SecretKeyAwsS3
//    ),
//    Region = RegionEndpoint.USEast1 // Thay đổi RegionEndpoint theo nhu cầu của bạn
// };

// awsOptions.DefaultClientConfig.ServiceURL = EnviConfig.ServiceURLAwsS3;

// builder.Services.AddDefaultAWSOptions(awsOptions);
// builder.Services.AddAWSService<IAmazonS3>(new AWSOptions
// {
//    DefaultClientConfig = s3Config
// });

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
app.UseAuthentication();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<ChatHub>("/chatHub");
});

app.MapControllers();

app.Run();
