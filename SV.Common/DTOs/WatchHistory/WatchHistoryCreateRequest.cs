using System;

namespace SV.Common.DTOs
{
    public class WatchHistoryCreateRequest
    {
        public string UserGuid { get; set; } = string.Empty;
        public string MovieGuid { get; set; } = string.Empty;
        public DateTime WatchedDate { get; set; }
        public int WatchMinutes { get; set; }
        public string DeviceType { get; set; } = string.Empty;
        public int PlayheadSeconds { get; set; }
        public bool IsFinished { get; set; }
    }
}
