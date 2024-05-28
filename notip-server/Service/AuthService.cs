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
        private readonly DbChatContext chatContext;
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
                throw new Exception("Email không tồn tại");
            }

            // 2. Validate the password is correct
            if (!_passwordService.VerifyPassword(user.PasswordHash, user.PasswordSalt, request.Password))
            {
                throw new Exception("Mật khẩu không chính xác.");
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
                    new Claim(ClaimTypes.Sid, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Expiration, expiresAt.ToString())

                }),
                Expires = expirationDate,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);

            return new AccessToken
            {
                Id = user.Id,
                UserName = user.UserName,
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
                throw new Exception("Email đã tồn tại");

            var saltPassword = _passwordService.GenerateSalt();
            var hashPassword = _passwordService.HashPassword(request.Password, saltPassword);

            User newUser = new User()
            {
                UserName = request.UserName,
                Email = request.Email,
                PhoneNumber = request.Phone,
                PasswordSalt = saltPassword,
                PasswordHash = hashPassword,
                Avatar = Constants.AVATAR_DEFAULT,
            };

            await chatContext.Users.AddAsync(newUser);
            await chatContext.SaveChangesAsync();
        }
    }
}
