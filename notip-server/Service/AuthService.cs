using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using notip_server.Data;
using notip_server.Dto;
using notip_server.Models;
using notip_server.Interfaces;
using notip_server.Utils;
using notip_server.ViewModel.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace notip_server.Service
{
    public class AuthService : IAuthService
    {
        protected readonly DbChatContext chatContext;
        protected readonly IPasswordService _passwordService;

        public AuthService(DbChatContext chatContext, IPasswordService passwordService)
        {
            this.chatContext = chatContext;
            _passwordService = passwordService;
        }

        /// <summary>
        /// Đăng nhập hệ thống
        /// </summary>
        /// <param name="user">Thông tin tài khoản người dùng</param>
        /// <returns>AccessToken</returns>
        public async Task<AccessToken> Login(LoginRequest request)
        {
            // 1. Validate the user exists
            if (await chatContext.Users.FirstOrDefaultAsync(x => x.Email == request.Email) is not User user)
            {
                throw new ArgumentException("Email không tồn tại");
            }

            // 2. Validate the password is correct
            if (!_passwordService.VerifyPassword(user.PasswordHash, user.PasswordSalt, request.Password))
            {
                throw new Exception("Invalid password.");
            }

            user.LastLogin = DateTime.Now;
            await chatContext.SaveChangesAsync();

            DateTime expirationDate = DateTime.Now.Date.AddMinutes(EnviConfig.ExpirationInMinutes);
            long expiresAt = (long)(expirationDate - new DateTime(1970, 1, 1)).TotalSeconds;

            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(EnviConfig.SecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Sid, user.Code),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Expiration, expiresAt.ToString())

                }),
                Expires = expirationDate,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);

            return new AccessToken
            {
                User = user.Code,
                FullName = user.FullName,
                Avatar = user.Avatar,
                Token = jwtTokenHandler.WriteToken(token),

            };
        }

        /// <summary>
        /// Đăng ký tài khoản người dùng
        /// </summary>
        /// <param name="user">Thông tin tài khoản</param>
        public async Task SignUp(SignUpRequest request)
        {
            if (await chatContext.Users.AnyAsync(x => x.Email.Equals(request.Email)))
                throw new ArgumentException("Tài khoản đã tồn tại");

            var saltPassword = _passwordService.GenerateSalt();
            var hashPassword = _passwordService.HashPassword(request.Password, saltPassword);

            User newUser = new User()
            {
                Code = Guid.NewGuid().ToString("N"),
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                PasswordSalt = saltPassword,
                PasswordHash = hashPassword,
                Avatar = Constants.AVATAR_DEFAULT,
            };

            await chatContext.Users.AddAsync(newUser);
            await chatContext.SaveChangesAsync();
        }

        /// <summary>
        /// Cập nhật thông tin hubconnection. Sử dụng khi thông báo riêng cho từng cá nhân.
        /// </summary>
        /// <param name="userSession">User hiện tại đang đăng nhập</param>
        /// <param name="key">HubConnection</param>
        public async Task PutHubConnection(string userSession, string key)
        {
            User user = await chatContext.Users
                .FirstOrDefaultAsync(x => x.Code.Equals(userSession));

            if (user != null)
            {
                user.CurrentSession = key;
                await chatContext.SaveChangesAsync();
            }
        }
    }
}
