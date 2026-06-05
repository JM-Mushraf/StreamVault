using SV.Common.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SV.Service.Abstractions
{
    public interface IReviewService
    {
        Task AddReviewAsync(int userId, int movieId, decimal rating, string? reviewText);
        Task<List<SV.Common.DTOs.ReviewCreateRequest>> GetMovieReviewsAsync(string movieGuid);
    }
}