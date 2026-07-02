using System.Collections.Generic;

namespace SV.Common.Models
{
    public class PagedResult<T>
    {
        public int TotalCount { get; set; }
        public List<T> Items { get; set; } = new List<T>();
    }
}
