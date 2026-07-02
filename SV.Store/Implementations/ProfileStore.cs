using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SV.Data.Connections;
using SV.Store.Abstractions;
using SV.Common.DTOs.Profile;

namespace SV.Store.Implementations
{
    public class ProfileStore : IProfileStore
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ProfileStore(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<ProfileResponseDto>> GetByAccountAsync(string userGuid)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            var query = "SELECT ProfileGuid, UserGuid, ProfileName, AvatarUrl, IsKids, CreatedOn FROM mst_Profile WHERE UserGuid = @UserGuid AND IsActive = 1 ORDER BY CreatedOn ASC";
            var rows = await conn.QueryAsync<ProfileResponseDto>(query, new { UserGuid = userGuid });
            return rows.ToList();
        }

        public async Task<string?> CreateAsync(string userGuid, CreateProfileDto dto, string createdBy)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            var guid = Guid.NewGuid().ToString("N");
            var query = @"
                INSERT INTO mst_Profile (ProfileGuid, UserGuid, ProfileName, AvatarUrl, IsKids, IsActive, CreatedOn, CreatedBy)
                VALUES (@ProfileGuid, @UserGuid, @ProfileName, @AvatarUrl, @IsKids, 1, GETDATE(), @CreatedBy)";

            var rowsAffected = await conn.ExecuteAsync(query, new {
                ProfileGuid = guid,
                UserGuid = userGuid,
                ProfileName = dto.ProfileName,
                AvatarUrl = dto.AvatarUrl,
                IsKids = dto.IsKids,
                CreatedBy = createdBy
            });

            return rowsAffected > 0 ? guid : null;
        }

        public async Task<bool> UpdateAsync(string profileGuid, CreateProfileDto dto, string updatedBy)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            var query = @"
                UPDATE mst_Profile 
                SET ProfileName = @ProfileName, AvatarUrl = @AvatarUrl, IsKids = @IsKids, UpdatedOn = GETDATE(), UpdatedBy = @UpdatedBy 
                WHERE ProfileGuid = @ProfileGuid AND IsActive = 1";

            var rowsAffected = await conn.ExecuteAsync(query, new {
                ProfileName = dto.ProfileName,
                AvatarUrl = dto.AvatarUrl,
                IsKids = dto.IsKids,
                UpdatedBy = updatedBy,
                ProfileGuid = profileGuid
            });

            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(string profileGuid, string updatedBy)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            var query = "UPDATE mst_Profile SET IsActive = 0, UpdatedOn = GETDATE(), UpdatedBy = @UpdatedBy WHERE ProfileGuid = @ProfileGuid";
            var rowsAffected = await conn.ExecuteAsync(query, new { ProfileGuid = profileGuid, UpdatedBy = updatedBy });
            return rowsAffected > 0;
        }

        public async Task<int> GetCountByAccountAsync(string userGuid)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            return await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM mst_Profile WHERE UserGuid = @UserGuid AND IsActive = 1", new { UserGuid = userGuid });
        }

        public async Task<ProfileResponseDto?> GetByGuidAsync(string profileGuid)
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn.State == ConnectionState.Closed) conn.Open();

            var query = "SELECT ProfileGuid, UserGuid, ProfileName, AvatarUrl, IsKids, CreatedOn FROM mst_Profile WHERE ProfileGuid = @ProfileGuid AND IsActive = 1";
            return await conn.QueryFirstOrDefaultAsync<ProfileResponseDto>(query, new { ProfileGuid = profileGuid });
        }
    }
}
