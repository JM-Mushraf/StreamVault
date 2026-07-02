using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SV.Data.Connections;
using SV.Store.Abstractions;
using SV.Common.DTOs.Notification;

namespace SV.Store.Implementations
{
    public class NotificationStore : INotificationStore
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public NotificationStore(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<NotificationResponseDto>> GetByUserAsync(string userGuid)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            string query = "SELECT NotificationGuid, UserGuid, Title, Message, IsRead, CreatedOn FROM tbl_Notification WHERE UserGuid = @UserGuid AND IsActive = 1 ORDER BY CreatedOn DESC";
            var rows = await conn.QueryAsync<NotificationResponseDto>(query, new { UserGuid = userGuid });
            return rows.ToList();
        }

        public async Task<string> CreateAsync(string userGuid, string title, string message, string createdBy)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            var guid = Guid.NewGuid().ToString("N");
            string query = @"
                INSERT INTO tbl_Notification (NotificationGuid, UserGuid, Title, Message, IsRead, CreatedOn, CreatedBy, IsActive)
                VALUES (@Guid, @UserGuid, @Title, @Message, 0, GETDATE(), @CreatedBy, 1)";

            await conn.ExecuteAsync(query, new {
                Guid = guid,
                UserGuid = userGuid,
                Title = title,
                Message = message,
                CreatedBy = createdBy
            });

            return guid;
        }

        public async Task<bool> MarkAsReadAsync(string notificationGuid)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            string query = "UPDATE tbl_Notification SET IsRead = 1 WHERE NotificationGuid = @Guid";
            var rows = await conn.ExecuteAsync(query, new { Guid = notificationGuid });
            return rows > 0;
        }

        public async Task<List<string>> GetAllUserGuidsAsync()
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            var rows = await conn.QueryAsync<string>("SELECT UserGuid FROM mst_User WHERE IsActive = 1");
            return rows.ToList();
        }
    }
}
