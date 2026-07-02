using System.Collections.Generic;
using System.Threading.Tasks;
using SV.Common.DTOs.Notification;
using SV.Common.Models;

namespace SV.Service.Abstractions
{
    public interface INotificationService
    {
        Task<ApiResponse<List<NotificationResponseDto>>> GetUserNotificationsAsync(string userGuid);
        Task<ApiResponse<bool>> MarkAsReadAsync(string notificationGuid);
        Task BroadcastNotificationAsync(string title, string message, string createdBy);
    }
}
