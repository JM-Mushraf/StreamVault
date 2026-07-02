using System;

namespace SV.Common.DTOs.Notification
{
    public class NotificationResponseDto
    {
        public string NotificationGuid { get; set; } = string.Empty;
        public string UserGuid { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
