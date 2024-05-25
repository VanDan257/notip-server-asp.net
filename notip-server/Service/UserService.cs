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

namespace notip_server.Service
{
    public class UserService : IUserService
    {
        private readonly DbChatContext chatContext;
        //protected readonly IWebHostEnvironment webHostEnvironment;
        public UserService(DbChatContext chatContext)
        {
            this.chatContext = chatContext;
            //this.webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Lấy thông tin cá nhân của user
        /// </summary>
        /// <param name="userCode">User hiện tại đang đăng nhập</param>
        /// <returns>Thông tin user</returns>
        public async Task<UserDto> GetProfile(string userCode)
        {
            return await chatContext.Users
                    .Where(x => x.Code.Equals(userCode))
                    .Select(x => new UserDto()
                    {
                        Code = x.Code,
                        FullName = x.FullName,
                        Address = x.Address,
                        Avatar = x.Avatar,
                        Email = x.Email,
                        Gender = x.Gender,
                        Phone = x.Phone,
                        Dob = x.Dob
                    }).FirstOrDefaultAsync();
        }


        /// <summary>
        /// Cập nhật thông tin cá nhân
        /// </summary>
        /// <param name="userCode">User hiện tại đang đăng nhập</param>
        /// <param name="user">Thông tin user</param>
        /// <returns></returns>
        public async Task<UserDto> UpdateProfile(string userCode, UpdateProfileRequest request)
        {
            User us = await chatContext.Users
                    .FirstOrDefaultAsync(x => x.Code.Equals(userCode));
            
            if (us != null)
            {
                us.FullName = request.FullName;
                us.Dob = request.Dob;
                us.Address = request.Address;
                us.Phone = request.Phone;
                us.Gender = request.Gender;

                await chatContext.SaveChangesAsync();
            }

            return new UserDto
            {
                Code = us.Code,
                FullName = us.FullName,
                Dob = us.Dob,
                Phone = us.Phone,
                Gender = us.Gender,
                Email = us.Email,
                Address = us.Address,
                Avatar = us.Avatar,
            };
        }


        /// <summary>
        /// Thêm mới liên hệ
        /// </summary>
        /// <param name="userCode">User hiện tại đang đăng nhập</param>
        /// <param name="user">Thông tin liên hệ</param>
        public async Task AddContact(string userCode, UserDto user)
        {
            Contact contact = new Contact()
            {
                UserCode = userCode,
                ContactCode = user.Code,
                Created = DateTime.Now
            };
            await chatContext.Contacts.AddAsync(contact);

            await chatContext.SaveChangesAsync();
        }

        /// <summary>
        /// Lấy thông tin người dùng theo code
        /// </summary>
        /// <param name="userCode"></param>
        /// <returns></returns>
        public async Task<PagingResult<FriendResponse>> GetContact(string userSession, GetContactRequest request)
        {
            try
            {

                var query = chatContext.Users.AsQueryable();

                if (!string.IsNullOrEmpty(request.UserCode))
                {
                    query = query.Where(x => x.Code == request.UserCode);
                }
                else if (!string.IsNullOrEmpty(request.KeySearch))
                {
                    query.Where(x => x.Code != userSession);
                        //.Where(x => x.FullName.Contains(request.KeySearch));
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
                        Code = x.Code,
                        FullName = x.FullName,
                        Address = x.Address,
                        Dob = x.Dob,
                        Email = x.Email,
                        Gender = x.Gender,
                        Phone = x.Phone
                    })
                    .ToListAsync();

                if (users != null)
                {
                    foreach(var user in users)
                    {
                        var friend = await chatContext.Friends.FirstOrDefaultAsync(x => x.SenderCode == userSession && x.ReceiverCode == user.Code);
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
                            var friend1 = await chatContext.Friends.FirstOrDefaultAsync(x => x.SenderCode == user.Code && x.ReceiverCode == userSession);
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
    }
}
