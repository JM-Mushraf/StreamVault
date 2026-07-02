using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using SV.Service.Abstractions;
using SV.Common.DTOs.Dashboard;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;


namespace SV.Service.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly SV.Store.Abstractions.IDashboardStore _dashboardStore;
        private readonly IMemoryCache _cache;

        private static int? ToNullableInt(object? o)
        {
            if (o == null) return null;
            try { return Convert.ToInt32(o); } catch { return null; }
        }

        private static decimal? ToNullableDecimal(object? o)
        {
            if (o == null) return null;
            try { return Convert.ToDecimal(o); } catch { return null; }
        }

        private static bool TryGetRaw(object row, string name, out object? value)
        {
            value = null;
            if (row == null) return false;
            // support both generic and non-generic dictionary shapes (ExpandoObject, Dapper row)
            if (row is System.Collections.Generic.IDictionary<string, object> gdict)
            {
                foreach (var kv in gdict)
                {
                    if (string.Equals(kv.Key, name, StringComparison.OrdinalIgnoreCase))
                    {
                        value = kv.Value;
                        return true;
                    }
                }
            }
            if (row is System.Collections.IDictionary dict)
            {
                foreach (System.Collections.DictionaryEntry de in dict)
                {
                    if (string.Equals(de.Key?.ToString(), name, StringComparison.OrdinalIgnoreCase))
                    {
                        value = de.Value;
                        return true;
                    }
                }
            }
            else
            {
                var prop = row.GetType().GetProperties().FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
                if (prop != null)
                {
                    value = prop.GetValue(row);
                    return true;
                }
            }
            return false;
        }

        private static string? GetFirstStringValue(object row)
        {
            if (row == null) return null;
            // try generic dict
            if (row is IDictionary<string, object> gdict)
            {
                foreach (var kv in gdict)
                {
                    if (kv.Value != null) return kv.Value.ToString();
                }
            }
            if (row is System.Collections.IDictionary dict)
            {
                foreach (System.Collections.DictionaryEntry de in dict)
                {
                    if (de.Value != null) return de.Value.ToString();
                }
            }
            var props = row.GetType().GetProperties();
            if (props.Length > 0)
            {
                var val = props[0].GetValue(row);
                return val?.ToString();
            }
            return null;
        }

        private static T? GetValue<T>(object row, params string[] names)
        {
            object? raw = null;
            foreach (var n in names)
            {
                if (TryGetRaw(row, n, out raw)) break;
            }
            if (raw == null) return default;
            try
            {
                if (typeof(T) == typeof(string))
                {
                    return (T)(object)(raw?.ToString() ?? string.Empty);
                }
                if (typeof(T) == typeof(int?) )
                {
                    return (T)(object)ToNullableInt(raw);
                }
                if (typeof(T) == typeof(decimal?))
                {
                    return (T)(object)ToNullableDecimal(raw);
                }
                return (T)Convert.ChangeType(raw, typeof(T));
            }
            catch
            {
                return default;
            }
        }

        public DashboardService(SV.Store.Abstractions.IDashboardStore dashboardStore, IMemoryCache cache)
        {
            _dashboardStore = dashboardStore;
            _cache = cache;
        }
        public async Task<object> GetAdminDashboardAsync()
        {
            // Cache admin aggregates for 60 seconds
            var cacheKey = "admin_dashboard";
            if (_cache.TryGetValue(cacheKey, out AdminDashboardDto cached))
            {
                return cached;
            }

            var rows = await _dashboardStore.GetAdminAsync();
            // rows is an object (list of dynamics), map to DTO

            var list = (rows as System.Collections.IEnumerable)?.Cast<object>().Select(r =>
            {
                return new AdminDashboardRowDto
                {
                    MetricCategory = GetValue<string>(r, "MetricCategory", "Metric", "Category") ?? string.Empty,
                    TotalUsers = GetValue<int?>(r, "TotalUsers", "Value1", "Count"),
                    NewUsersInRange = GetValue<int?>(r, "NewUsersInRange", "Value2"),
                    ActiveUsers = GetValue<int?>(r, "ActiveUsers", "Value3")
                };
            }).ToList();

            var dto = new AdminDashboardDto { Rows = list ?? new System.Collections.Generic.List<AdminDashboardRowDto>() };

            _cache.Set(cacheKey, dto, TimeSpan.FromSeconds(60));
            return dto;
        }

        public async Task<object> GetUserDashboardAsync(string userGuid)
        {
            // Return raw stored-procedure result for user dashboard without mapping
            var rows = await _dashboardStore.GetUserAsync(userGuid);
            return rows;
        }
    }
}
