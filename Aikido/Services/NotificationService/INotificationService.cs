using Aikido.AdditionalData.Enums;

namespace Aikido.Services.NotificationService
{
    public interface INotificationService
    {
        Task DataChanged(string entity, DbAction action);
    }
}
