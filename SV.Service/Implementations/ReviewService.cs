using System.Collections.Generic;
using System.Threading.Tasks;
using SV.Service.Abstractions;
using SV.Common.DTOs;
using System.Linq;

namespace SV.Service.Implementations
{
    public class ReviewService : IReviewService
    {
        private readonly SV.Store.Abstractions.IReviewStore _store;
        private readonly SV.Service.Abstractions.IMovieService _movieService;

        public ReviewService(SV.Store.Abstractions.IReviewStore store, SV.Service.Abstractions.IMovieService movieService)
        {
            _store = store;
            _movieService = movieService;
        }

        public Task AddReviewAsync(string userGuid, string movieGuid, int rating, string? reviewText, string createdBy, string? profileGuid = null)
        {
            return _store.AddAsync(userGuid, movieGuid, rating, reviewText, createdBy, profileGuid);
        }

        public Task<List<SV.Common.DTOs.ReviewCreateRequest>> GetMovieReviewsAsync(string movieGuid)
        {
            return Task.FromResult(new List<SV.Common.DTOs.ReviewCreateRequest>());
        }

        public async Task<object?> GetByMovieGuidAsync(string movieGuid)
        {
            var movie = await _movieService.GetByGuidAsync(movieGuid);
            var rows = await _store.GetByMovieAsync(movieGuid);

            var items = rows.Select(r =>
            {
                dynamic d = r;
                int rating = 0;
                string? reviewText = null;
                string userFullName = string.Empty;
                string createdOn = string.Empty;
                string? profileGuid = null;

                try { rating = d.Rating != null ? (int)d.Rating : 0; } catch { rating = 0; }
                try { reviewText = d.ReviewText != null ? (string)d.ReviewText : null; } catch { reviewText = null; }
                try { userFullName = d.UserFullName != null ? (string)d.UserFullName : string.Empty; } catch { userFullName = string.Empty; }
                try { createdOn = d.CreatedOn != null ? d.CreatedOn.ToString() : string.Empty; } catch { createdOn = string.Empty; }
                try { profileGuid = d.ProfileGuid != null ? (string)d.ProfileGuid : null; } catch { profileGuid = null; }

                return (object)new
                {
                    Rating = rating,
                    ReviewText = reviewText,
                    UserFullName = userFullName,
                    CreatedOn = createdOn,
                    ProfileGuid = profileGuid
                };
            }).ToList();

            var response = new { Movie = movie, Reviews = items };
            return response;
        }
    }
}
