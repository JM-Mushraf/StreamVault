using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using ProjectFileStructure.Hubs;
using SV.Common.DTOs.Notification;
using SV.Common.Models;
using SV.Service.Abstractions;
using SV.Store.Abstractions;

namespace ProjectFileStructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationStore _store;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(INotificationStore store, IHubContext<NotificationHub> hubContext)
        {
            _store = store;
            _hubContext = hubContext;
        }

        public async Task<ApiResponse<List<NotificationResponseDto>>> GetUserNotificationsAsync(string userGuid)
        {
            var list = await _store.GetByUserAsync(userGuid);
            return new ApiResponse<List<NotificationResponseDto>>
            {
                Success = true,
                Message = "Notifications retrieved successfully.",
                Data = list
            };
        }

        public async Task<ApiResponse<bool>> MarkAsReadAsync(string notificationGuid)
        {
            var success = await _store.MarkAsReadAsync(notificationGuid);
            return new ApiResponse<bool>
            {
                Success = success,
                Message = success ? "Notification marked as read." : "Notification not found.",
                Data = success
            };
        }

        public async Task BroadcastNotificationAsync(string title, string message, string createdBy)
        {
            var users = await _store.GetAllUserGuidsAsync();

            foreach (var userGuid in users)
            {
                var guid = await _store.CreateAsync(userGuid, title, message, createdBy);

                await _hubContext.Clients.Group(userGuid).SendAsync("ReceiveNotification", new
                {
                    NotificationGuid = guid,
                    UserGuid = userGuid,
                    Title = title,
                    Message = message,
                    IsRead = false,
                    CreatedOn = DateTime.UtcNow
                });
            }

            await _hubContext.Clients.All.SendAsync("ReceiveGlobalNotification", new
            {
                Title = title,
                Message = message,
                CreatedOn = DateTime.UtcNow
            });
        }
    }
}
