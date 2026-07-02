using SV.Common.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SV.Service.Abstractions
{
    public interface IReviewService
    {
        Task AddReviewAsync(string userGuid, string movieGuid, int rating, string? reviewText, string createdBy, string? profileGuid = null);
        Task<List<SV.Common.DTOs.ReviewCreateRequest>> GetMovieReviewsAsync(string movieGuid);
        Task<object?> GetByMovieGuidAsync(string movieGuid);
    }
}