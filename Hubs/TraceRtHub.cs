using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace webHttpTest.Hubs
{
    public class TraceRtHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}