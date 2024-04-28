using notip_server.Interfaces;
using notip_server.Service;

namespace notip_server.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICallService, CallService>();
            services.AddScoped<IChatBoardService, ChatBoardService>();
            services.AddScoped<IUserService, UserService>();
            services.AddTransient<IPasswordService, PasswordService>();

            return services;
        }
    }
}
