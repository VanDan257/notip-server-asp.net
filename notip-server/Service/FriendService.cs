using Microsoft.EntityFrameworkCore;
using notip_server.Data;
using notip_server.Interfaces;
using notip_server.Utils;

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
        public async Task AcceptFriendRequest(int requestId)
        {
            try
            {
                var friend = await _chatContext.Friends.FirstOrDefaultAsync(x => x.Id == requestId);
                if (friend != null)
                {
                    friend.Status = Constants.FriendStatus.FRIEND;
                    _chatContext.Update(friend);

                    await _chatContext.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Có lỗi xảy ra!");
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
        public async Task CancelFriendRequest(int requestId)
        {
            try
            {
                var friend = await _chatContext.Friends.FirstOrDefaultAsync(x => x.Id == requestId);
                if(friend != null)
                {
                    _chatContext.Remove(friend);

                    await _chatContext.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Có lỗi xảy ra!");
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
        public async Task UnBlockUser(int requestId)
        {
            try
            {
                var block = await _chatContext.Friends.FirstOrDefaultAsync(x => x.Id == requestId);
                if(block != null)
                {
                    _chatContext.Friends.Remove(block);
                }
                else
                {
                    throw new Exception("Có lỗi xảy ra!");
                }

                await _chatContext.SaveChangesAsync();
                
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
