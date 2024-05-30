using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using notip_server.Data;
using notip_server.Hubs;
using notip_server.Interfaces;
using notip_server.Models;
using notip_server.Service;

namespace notip_server.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Thêm Identity
            services.AddIdentity<User, Role>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 1;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.User.AllowedUserNameCharacters = null;
            }).AddEntityFrameworkStores<DbChatContext>().AddDefaultTokenProviders();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICallService, CallService>();
            services.AddScoped<IChatBoardService, ChatBoardService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IFriendService, FriendService>();
            services.AddTransient<IPasswordService, PasswordService>();

            return services;
        }
    }
}
