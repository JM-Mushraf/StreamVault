using System.Collections.Generic;
using System.Threading.Tasks;
using SV.Common.DTOs.Notification;

namespace SV.Store.Abstractions
{
    public interface INotificationStore
    {
        Task<List<NotificationResponseDto>> GetByUserAsync(string userGuid);
        Task<string> CreateAsync(string userGuid, string title, string message, string createdBy);
        Task<bool> MarkAsReadAsync(string notificationGuid);
        Task<List<string>> GetAllUserGuidsAsync();
    }
}
