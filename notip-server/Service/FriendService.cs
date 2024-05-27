using Microsoft.EntityFrameworkCore;
using notip_server.Data;
using notip_server.Interfaces;
using notip_server.Models;
using notip_server.Utils;
using notip_server.ViewModel.Common;
using notip_server.ViewModel.Friend;
using notip_server.ViewModel.User;

namespace notip_server.Service
{
    public class FriendService : IFriendService
    {
        private readonly DbChatContext _chatContext;

        public FriendService(DbChatContext chatContext)
        {
            _chatContext = chatContext;
        }

        /// <summary>
        /// Lấy danh sách contact là bạn bè với user hiện tại
        /// </summary>
        /// <param name="userSession"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<PagingResult<FriendResponse>> GetListFriend(string userSession, GetContactRequest request)
        {
            var query = from user in _chatContext.Users
                        join friend in _chatContext.Friends
                        on new { Code = user.Code } equals new { Code = friend.SenderCode } into friendsGroup
                        from friend in friendsGroup.DefaultIfEmpty()
                        where (user.Code == friend.SenderCode || user.Code == friend.ReceiverCode)
                                && friend.Status == Constants.FriendStatus.FRIEND
                        select new FriendResponse
                        {
                            Code = user.Code,
                            FullName = user.FullName,
                            Dob = user.Dob,
                            Phone = user.Phone,
                            Email = user.Code,
                            Address = user.Address,
                            Avatar = user.Avatar,
                            IsFriend = true
                        };
            int total = await query.CountAsync();

            if (request.PageIndex == null || request.PageIndex == 0) request.PageIndex = 1;
            if (request.PageSize == null || request.PageSize == 0) request.PageSize = total;

            int totalPages = (int)Math.Ceiling((double)total / request.PageSize.Value);

            var friends = await query
                .Skip((request.PageIndex.Value - 1) * request.PageSize.Value)
                .Take(request.PageSize.Value)
                .OrderBy(x => x.FullName)
            .ToListAsync();

            return new PagingResult<FriendResponse>(friends, request.PageIndex.Value, request.PageSize.Value, total, totalPages);
        }

        /// <summary>
        /// Lấy danh sách contact gửi lời mời kết bạn cho người dùng hiện tại
        /// </summary>
        /// <param name="userSession"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<PagingResult<FriendResponse>> GetListFriendInvite(string userSession, GetContactRequest request)
        {
            var query = from user in _chatContext.Users
                        join friend in _chatContext.Friends
                        on user.Code equals friend.SenderCode
                        where friend.ReceiverCode == userSession
                                && friend.Status == Constants.FriendStatus.FRIENDREQUEST
                        select new FriendResponse
                        {
                            Code = user.Code,
                            FullName = user.FullName,
                            Dob = user.Dob,
                            Phone = user.Phone,
                            Email = user.Code,
                            Address = user.Address,
                            Avatar = user.Avatar,
                            IsBeenSentFriend = true
                        };
            int total = await query.CountAsync();

            if (request.PageIndex == null || request.PageIndex == 0) request.PageIndex = 1;
            if (request.PageSize == null || request.PageSize == 0) request.PageSize = total;

            int totalPages = (int)Math.Ceiling((double)total / request.PageSize.Value);

            var friends = await query
                .Skip((request.PageIndex.Value - 1) * request.PageSize.Value)
                .Take(request.PageSize.Value)
                .OrderBy(x => x.FullName)
            .ToListAsync();

            return new PagingResult<FriendResponse>(friends, request.PageIndex.Value, request.PageSize.Value, total, totalPages);
        }

        /// <summary>
        /// Gửi yêu cầu kết bạn
        /// </summary>
        /// <param name="userSession"></param>
        /// <param name="receiverCode"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task SendFriendRequest(string userSession, string receiverCode)
        {
            try
            {
                var receiver = await _chatContext.Users.FirstOrDefaultAsync(x => x.Code == receiverCode);

                if(receiver != null)
                {
                    await _chatContext.Friends.AddAsync(new Models.Friend
                    {
                        SenderCode = userSession,
                        ReceiverCode = receiverCode,
                        Status = Constants.FriendStatus.FRIENDREQUEST
                    });

                    await _chatContext.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Không tìm thấy người dùng!");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Chấp nhận lời mời kết bạn
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task AcceptFriendRequest(string userSession, string receiverCode)
        {
            try
            {
                var friend = await _chatContext.Friends.FirstOrDefaultAsync(x => x.SenderCode == userSession && x.ReceiverCode == receiverCode);
                if (friend != null)
                {
                    friend.Status = Constants.FriendStatus.FRIEND;
                    _chatContext.Update(friend);

                    await _chatContext.SaveChangesAsync();
                }
                else
                {
                    var friend1 = await _chatContext.Friends.FirstOrDefaultAsync(x => x.SenderCode == receiverCode && x.ReceiverCode == userSession);
                    if(friend1 != null)
                    {

                        friend1.Status = Constants.FriendStatus.FRIEND;
                        _chatContext.Update(friend);

                        await _chatContext.SaveChangesAsync();
                    }
                    else
                    {
                        throw new Exception("Có lỗi xảy ra!");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Huỷ yêu cầu kết bạn
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task CancelFriendRequest(string userSession, string receiverCode)
        {
            try
            {
                var friend = await _chatContext.Friends.FirstOrDefaultAsync(x => x.SenderCode == userSession && x.ReceiverCode == receiverCode);
                if (friend != null)
                {
                    _chatContext.Remove(friend);

                    await _chatContext.SaveChangesAsync();
                }
                else
                {
                    var friend1 = await _chatContext.Friends.FirstOrDefaultAsync(x => x.SenderCode == receiverCode && x.ReceiverCode == userSession);
                    if (friend1 != null)
                    {
                        _chatContext.Remove(friend);

                        await _chatContext.SaveChangesAsync();
                    }
                    else
                    {
                        throw new Exception("Có lỗi xảy ra!");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Chặn người dùng
        /// </summary>
        /// <param name="userSession"></param>
        /// <param name="receiverCode"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task BlockUser(string userSession, string receiverCode)
        {
            try
            {
                var receiver = await _chatContext.Users.FirstOrDefaultAsync(x => x.Code == receiverCode);

                if (receiver != null)
                {
                    await _chatContext.Friends.AddAsync(new Models.Friend
                    {
                        SenderCode = userSession,
                        ReceiverCode = receiverCode,
                        Status = Constants.FriendStatus.BLOCKED
                    });

                    await _chatContext.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Không tìm thấy người dùng!");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Bỏ chặn người dùng
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task UnBlockUser(string userSession, string receiverCode)
        {
            try
            {
                var friend = await _chatContext.Friends.FirstOrDefaultAsync(x => x.SenderCode == userSession && x.ReceiverCode == receiverCode);
                if (friend != null)
                {
                    _chatContext.Remove(friend);

                    await _chatContext.SaveChangesAsync();
                }
                else
                {
                    var friend1 = await _chatContext.Friends.FirstOrDefaultAsync(x => x.SenderCode == receiverCode && x.ReceiverCode == userSession);
                    if (friend1 != null)
                    {
                        _chatContext.Remove(friend);

                        await _chatContext.SaveChangesAsync();
                    }
                    else
                    {
                        throw new Exception("Có lỗi xảy ra!");
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
