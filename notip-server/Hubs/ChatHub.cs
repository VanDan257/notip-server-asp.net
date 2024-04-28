using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace notip_server.Hubs
{
    public class ChatHub : Hub
    {
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
