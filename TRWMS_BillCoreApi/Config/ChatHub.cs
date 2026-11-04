using System;
using Microsoft.AspNetCore.SignalR;

namespace test.webapi
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string message)
        {
            // Todolist ...
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}

