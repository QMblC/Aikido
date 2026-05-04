using Aikido.AdditionalData.Enums;
using Aikido.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Aikido.Services.NotificationService
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hub;

        public NotificationService(IHubContext<NotificationHub> hub)
        {
            _hub = hub;
        }

        public async Task DataChanged(string entity, DbAction action)
        {
            var actionInString = EnumParser.ConvertEnumToString(action);

            await _hub.Clients.All.SendAsync("dataChanged", new
            {
                entity,
                actionInString
            });
        }
    }
}
