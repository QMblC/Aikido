using Aikido.AdditionalData.Enums;
using Aikido.Hubs;
using DocumentFormat.OpenXml.Office2010.Excel;
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

        public async Task UserDataChanged(NotificationAction action, long? id = null)
        {
            await _hub.Clients.All.SendAsync("dataChanged", new
            {
                entity = EnumParser.ConvertEnumToString(NotificationEntityType.User).ToLowerInvariant(),
                id,
                action = EnumParser.ConvertEnumToString(action).ToLowerInvariant(),
            });
        }

        public async Task ClubDataChanged(NotificationAction action, long? id = null)
        {
             await _hub.Clients.All.SendAsync("dataChanged", new
            {
                entity = EnumParser.ConvertEnumToString(NotificationEntityType.Club).ToLowerInvariant(),
                id,
                action = EnumParser.ConvertEnumToString(action).ToLowerInvariant(),            
            });
        }

        public async Task GroupDataChanged(NotificationAction action, long? id = null)
        {
            await _hub.Clients.All.SendAsync("dataChanged", new
            {
                entity = EnumParser.ConvertEnumToString(NotificationEntityType.Group).ToLowerInvariant(),
                id,
                action = EnumParser.ConvertEnumToString(action).ToLowerInvariant(),
            });
        }

        public async Task GroupMembersDataChanged(NotificationAction action, long? id = null)
        {
            await _hub.Clients.All.SendAsync("dataChanged", new
            {
                entity = EnumParser.ConvertEnumToString(NotificationEntityType.GroupMembers).ToLowerInvariant(),
                id,
                action = EnumParser.ConvertEnumToString(action).ToLowerInvariant(),
            });
        }

        public async Task SeminarDataChanged(NotificationAction action, long? id = null)
        {
            await _hub.Clients.All.SendAsync("dataChanged", new
            {
                entity = EnumParser.ConvertEnumToString(NotificationEntityType.Seminar).ToLowerInvariant(),
                id,
                action = EnumParser.ConvertEnumToString(action).ToLowerInvariant(),
            });
        }

        public async Task SeminarManagerMembersDataChanged(NotificationAction action, 
            long seminarId, 
            long managerId,
            long clubId)
        {
            await _hub.Clients.All.SendAsync("dataChanged", new
            {
                entity = EnumParser.ConvertEnumToString(NotificationEntityType.SeminarManagerMembers).ToLowerInvariant(),
                seminarId,
                managerId,
                clubId,
                action = EnumParser.ConvertEnumToString(action).ToLowerInvariant(),
            });
        }

        public async Task SeminarCoachMembersDataChanged(NotificationAction action, 
            long seminarId,
            long coachId,
            long clubId)
        {
            await _hub.Clients.All.SendAsync("dataChanged", new
            {
                entity = EnumParser.ConvertEnumToString(NotificationEntityType.SeminarCoachMembers).ToLowerInvariant(),
                seminarId,
                coachId,
                action = EnumParser.ConvertEnumToString(action).ToLowerInvariant(),
            });
        } 

        public async Task SeminarMembersDataChanged(NotificationAction action, long seminarId)
        {
            await _hub.Clients.All.SendAsync("dataChanged", new
            {
                entity = EnumParser.ConvertEnumToString(NotificationEntityType.SeminarMembers).ToLowerInvariant(),
                seminarId,
                action = EnumParser.ConvertEnumToString(action).ToLowerInvariant(),
            });
        }
    }
}
