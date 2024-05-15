using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using notip_server.Data;
using notip_server.Dto;
using notip_server.Models;
using System.Collections.Concurrent;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace notip_server.Hubs
{
    public class ChatHub : Hub
    {
        private readonly DbChatContext _chatContext;

        public ChatHub(DbChatContext chatContext)
        {
            _chatContext = chatContext;
        }

        public static ConcurrentDictionary<string, string> users = new ConcurrentDictionary<string, string>();

        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }

        public override Task OnConnectedAsync()
        {
            users.TryAdd(Context.ConnectionId, Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            string user;
            users.TryRemove(Context.ConnectionId, out user);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task AddToGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task SendMessageToGroup(string groupCode, string userCurrentCode, Message message)
        {
            try
            {
                // lấy tất cả thành viên trong group trừ người dùng hiện tại 
                var members = await  _chatContext.GroupUsers
                            .Where(x => x.GroupCode == groupCode && x.UserCode != userCurrentCode)
                            .Join(_chatContext.Users,
                                grpUsers => grpUsers.UserCode,
                                users => users.Code,
                                (grpUsers, users) => new
                                {
                                    Code = users.Code,
                                    FullName = users.FullName,
                                    Dob = users.Dob,
                                    Phone = users.Phone,
                                    Email = users.Email,
                                    Address = users.Address,
                                    Avatar = users.Avatar,
                                    Gender = users.Gender,
                                    CurrentSession = users.CurrentSession

                                })
                            .ToListAsync();

                foreach(var member in members)
                {
                    if (!string.IsNullOrEmpty(member.CurrentSession))
                    {
                        await SendMessage(member.CurrentSession, message);
                    }
                }
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task SendMessage(string receiverSession, Message message)
        {
            try
            {
                if (users.TryGetValue(receiverSession, out string receiver))
                {

                    await Clients.Client(receiverSession).SendAsync("ReceiveMessage", message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        //test hub
        /*
        public async Task AskServer(string textFromClient)
        {
            string tempString;

            if (textFromClient == "hello")
                tempString = "Message was: xin chao...";
            else
                tempString = "Message was: tam biet";

            await Clients.Clients(this.Context.ConnectionId).SendAsync("askServerRespone", tempString);
        }
        */
    }
}
