using Microsoft.EntityFrameworkCore;
using notip_server.Data;
using notip_server.Dto;
using notip_server.Models;
using notip_server.Interfaces;
using notip_server.ViewModel.User;
using System.Numerics;
using System.Net.WebSockets;
using notip_server.Utils;
using notip_server.ViewModel.Friend;
using notip_server.ViewModel.Common;
using Microsoft.AspNetCore.Identity;

namespace notip_server.Service
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DbChatContext chatContext;
        private readonly ICommonService _commonService;
        public UserService(DbChatContext chatContext, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor, ICommonService commonService)
        {
            this.chatContext = chatContext;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _commonService = commonService;
        }

        /// <summary>
        /// Lấy thông tin cá nhân của user
        /// </summary>
        /// <param name="userCode">User hiện tại đang đăng nhập</param>
        /// <returns>Thông tin user</returns>
        public async Task<UserDto> GetProfile(Guid userCode)
        {
            return await chatContext.Users
                    .Where(x => x.Id.Equals(userCode))
                    .Select(x => new UserDto()
                    {
                        Id = x.Id,
                        UserName = x.UserName,
                        Address = x.Address,
                        Avatar = x.Avatar,
                        Email = x.Email,
                        Gender = x.Gender,
                        PhoneNumber = x.PhoneNumber,
                        Dob = x.Dob
                    }).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Cập nhật thông tin cá nhân
        /// </summary>
        /// <param name="userCode">User hiện tại đang đăng nhập</param>
        /// <param name="user">Thông tin user</param>
        /// <returns></returns>
        public async Task<UserDto> UpdateProfile(Guid userCode, UpdateProfileRequest request)
        {
            User us = await chatContext.Users
                    .FirstOrDefaultAsync(x => x.Id.Equals(userCode));
            
            if (us != null)
            {
                us.UserName = request.UserName;
                us.Dob = request.Dob;
                us.Address = request.Address;
                us.PhoneNumber = request.PhoneNumber;
                us.Gender = request.Gender;

                await chatContext.SaveChangesAsync();
            }

            return new UserDto
            {
                Id = us.Id,   
                UserName = us.UserName,
                Dob = us.Dob,
                PhoneNumber = us.PhoneNumber,
                Gender = us.Gender,
                Email = us.Email,
                Address = us.Address,
                Avatar = us.Avatar,
            };
        }

        public async Task UpdateAvatar(Guid userSessionId, UpdateAvatarRequest request)
        {
            try
            {
                var user = await chatContext.Users.FirstOrDefaultAsync(x => x.Id == userSessionId);
                if(user != null){
                    string pathFile = "Avatar";
                    await _commonService.UploadBlobFile(request.Image[0], pathFile);

                    user.Avatar = $"{pathFile}/{request.Image[0].FileName}";
                    chatContext.Users.Update(user);
                    await chatContext.SaveChangesAsync();
                }
            }
            catch(Exception ex){
                throw new Exception("Có lỗi xảy ra! Hãy thử lại!");
            }
        }

        /// <summary>
        /// Lấy thông tin người dùng theo code
        /// </summary>
        /// <param name="userCode"></param>
        /// <returns></returns>
        public async Task<PagingResult<FriendResponse>> GetContact(Guid userSession, GetContactRequest request)
        {
            try
            {
                var query = chatContext.Users.AsQueryable();

                if (!string.IsNullOrEmpty(request.UserCode.ToString()))
                {
                    query = query.Where(x => x.Id == request.UserCode);
                }
                else if (!string.IsNullOrEmpty(request.KeySearch))
                {
                    query = query.Where(x => x.Id != userSession)
                                 .Where(x => x.UserName.ToLower().Contains(request.KeySearch.ToLower()) || x.Email.ToLower().Contains(request.KeySearch.ToLower()) || x.PhoneNumber.Contains(request.KeySearch));
                }

                int total = await query.CountAsync();

                if (request.PageIndex == null || request.PageIndex == 0) request.PageIndex = 1;
                if (request.PageSize == null || request.PageSize == 0) request.PageSize = total;

                int totalPages = (int)Math.Ceiling((double)total / request.PageSize.Value);

                var users = await query
                    .Skip((request.PageIndex.Value - 1) * request.PageSize.Value)
                    .Take(request.PageSize.Value)
                    .Select(x => new FriendResponse()
                    {
                        Avatar = x.Avatar,
                        Id = x.Id,
                        UserName = x.UserName,
                        Address = x.Address,
                        Dob = x.Dob,
                        Email = x.Email,
                        Gender = x.Gender,
                        PhoneNumber = x.PhoneNumber
                    })
                    .ToListAsync();

                if (users != null)
                {
                    foreach(var user in users)
                    {
                        var friend = await chatContext.Friends.FirstOrDefaultAsync(x => x.SenderCode == userSession && x.ReceiverCode == user.Id);
                        if(friend != null)
                        {
                            switch (friend.Status)
                            {
                                case Constants.FriendStatus.FRIEND:
                                    user.IsFriend = true;
                                    break;

                                case Constants.FriendStatus.FRIENDREQUEST:
                                    user.IsSentFriend = true;
                                    break;

                                case Constants.FriendStatus.BLOCKED:
                                    user.IsBlocked = true;
                                    break;
                            }
                        }
                        else
                        {
                            var friend1 = await chatContext.Friends.FirstOrDefaultAsync(x => x.SenderCode == user.Id && x.ReceiverCode == userSession);
                            if (friend1 != null)
                            {
                                switch (friend1.Status)
                                {
                                    case Constants.FriendStatus.FRIEND:
                                        user.IsFriend = true;
                                        break;

                                    case Constants.FriendStatus.FRIENDREQUEST:
                                        user.IsBeenSentFriend = true;
                                        break;

                                    case Constants.FriendStatus.BLOCKED:
                                        user.IsBeenBlocked = true;
                                        break;
                                }
                            }
                        }
                    }

                    return new PagingResult<FriendResponse>(users, request.PageIndex.Value, request.PageSize.Value, total, totalPages);
                }
                else
                {
                    throw new Exception("Không tìm thấy người dùng");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<User?> GetCurrentUserAsync()
        {
            return await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
        }

    }
}
