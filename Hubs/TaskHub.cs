using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace JwtAuthDotnetEight.Hubs
{
    [Authorize]
    public class TaskHub : Hub
    {
    }
}
