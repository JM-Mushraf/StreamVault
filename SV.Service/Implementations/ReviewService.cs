using System.Collections.Generic;
using System.Threading.Tasks;
using SV.Service.Abstractions;

namespace SV.Service.Implementations
{
    public class ReviewService : IReviewService
    {
        public Task AddReviewAsync(int userId, int movieId, decimal rating, string? reviewText)
        {
            return Task.CompletedTask;
        }

        public Task<List<SV.Common.DTOs.ReviewCreateRequest>> GetMovieReviewsAsync(string movieGuid)
        {
            return Task.FromResult(new List<SV.Common.DTOs.ReviewCreateRequest>());
        }
    }
}
