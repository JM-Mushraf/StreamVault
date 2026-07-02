using System;
using System.Collections.Generic;

namespace SV.Common.DTOs.Dashboard
{
    public class AdminDashboardRowDto
    {
        public string MetricCategory { get; set; } = string.Empty;
        public int? TotalUsers { get; set; }
        public int? NewUsersInRange { get; set; }
        public int? ActiveUsers { get; set; }
    }

    public class AdminDashboardDto
    {
        public List<AdminDashboardRowDto> Rows { get; set; } = new List<AdminDashboardRowDto>();
    }

    public class UserSubscriptionDto
    {
        public string SubscriptionGuid { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string VideoQuality { get; set; } = string.Empty;
        public int? ScreenLimit { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string SubscriptionStatus { get; set; } = string.Empty;
        public decimal? MonthlyPrice { get; set; }
    }

    public class RecentWatchDto
    {
        public string MovieGuid { get; set; } = string.Empty;
        public string MovieName { get; set; } = string.Empty;
        public string GenreName { get; set; } = string.Empty;
        public DateTime? WatchedDate { get; set; }
        public int? WatchMinutes { get; set; }
        public int? DurationMinutes { get; set; }
        public string Language { get; set; } = string.Empty;
    }

    public class WatchlistDto
    {
        public string MovieGuid { get; set; } = string.Empty;
        public string MovieName { get; set; } = string.Empty;
        public string GenreName { get; set; } = string.Empty;
        public DateTime? AddedOn { get; set; }
        public int? DurationMinutes { get; set; }
        public string Language { get; set; } = string.Empty;
        public int? Rating { get; set; }
    }

    public class RecommendationDto
    {
        public string MovieGuid { get; set; } = string.Empty;
        public string MovieName { get; set; } = string.Empty;
        public string GenreName { get; set; } = string.Empty;
        public DateTime? ReleaseDate { get; set; }
        public int? DurationMinutes { get; set; }
        public string Language { get; set; } = string.Empty;
        public int? Rating { get; set; }
    }

    public class UserDashboardDto
    {
        public List<UserSubscriptionDto> Subscriptions { get; set; } = new List<UserSubscriptionDto>();
        public List<RecentWatchDto> RecentWatches { get; set; } = new List<RecentWatchDto>();
        public List<WatchlistDto> Watchlist { get; set; } = new List<WatchlistDto>();
        public List<RecommendationDto> Recommendations { get; set; } = new List<RecommendationDto>();
    }
}
