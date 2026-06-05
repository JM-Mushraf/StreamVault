using System;

namespace SV.Common.DTOs
{
    public class WatchHistoryCreateRequest
    {
        public int MovieId { get; set; }
        public DateTime WatchedDate { get; set; }
        public int WatchMinutes { get; set; }
        public string DeviceType { get; set; } = string.Empty;
    }
}
