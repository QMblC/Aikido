using Aikido.AdditionalData.Enums;

namespace Aikido.Services.NotificationService
{
    public interface INotificationService
    {
        Task UserDataChanged(NotificationAction action, long? id = null);
        Task ClubDataChanged(NotificationAction action, long? id = null);
        Task GroupDataChanged(NotificationAction action, long? id = null);
        Task GroupMembersDataChanged(NotificationAction action, long? id = null);
        Task SeminarDataChanged(NotificationAction action, long? id = null);
        Task SeminarManagerMembersDataChanged(NotificationAction action, long seminarId, long managerId, long clubId);
        Task SeminarCoachMembersDataChanged(NotificationAction action, long seminarId, long coachId, long clubId);
        Task SeminarMembersDataChanged(NotificationAction action, long seminarId);
    }
}
