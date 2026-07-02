using System.Collections.Generic;
using System.Threading.Tasks;
using SV.Common.DTOs.Movie;
using SV.Common.Abstractions;
using SV.Service.Abstractions;
using SV.Store.Abstractions;
using SV.Common.DTOs;
using SV.Common.Models;
using Microsoft.Extensions.DependencyInjection;

namespace SV.Service.Implementations
{
    public class MovieService : IMovieService
    {
        private readonly IMovieStore _store;
        private readonly SV.Common.Abstractions.ICloudinaryService _cloudinaryService;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly System.IServiceProvider _serviceProvider;

        public MovieService(
            IMovieStore store, 
            SV.Common.Abstractions.ICloudinaryService cloudinaryService,
            IBackgroundTaskQueue backgroundTaskQueue,
            System.IServiceProvider serviceProvider)
        {
            _store = store;
            _cloudinaryService = cloudinaryService;
            _backgroundTaskQueue = backgroundTaskQueue;
            _serviceProvider = serviceProvider;
        }

        public Task<List<object>> GetLatestMoviesAsync(string[]? genres = null)
        {
            return _store.GetLatestMoviesAsync(genres);
        }

        public async Task<string> CreateMovieAsync(CreateMovieRequest request, string createdBy, SV.Common.DTOs.FileUploadDto? video = null, SV.Common.DTOs.FileUploadDto? thumbnail = null)
        {
            string? thumbUrl = null;
            string? thumbPublicId = null;

            if (thumbnail != null)
            {
                var tres = await _cloudinaryService.UploadImageAsync(thumbnail, "streamvault/movies/thumbnails");
                thumbUrl = tres.SecureUrl;
                thumbPublicId = tres.PublicId;
            }

            string? initialTranscodeStatus = video != null ? "processing" : null;

            var movieGuid = await _store.CreateMovieAsync(request, createdBy, null, null, initialTranscodeStatus, null, thumbUrl, thumbPublicId);
            
            if (video != null && movieGuid != null)
            {
                var videoStream = new System.IO.MemoryStream();
                await video.Content.CopyToAsync(videoStream);
                videoStream.Position = 0;

                var videoFileDto = new FileUploadDto
                {
                    FileName = video.FileName,
                    ContentType = video.ContentType,
                    Content = videoStream
                };

                await _backgroundTaskQueue.QueueBackgroundWorkItemAsync(async token =>
                {
                    try
                    {
                        var vres = await _cloudinaryService.UploadVideoAsync(videoFileDto, "streamvault/movies", token);
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var scopedStore = scope.ServiceProvider.GetRequiredService<IMovieStore>();
                            await scopedStore.UpdateMovieVideoMetaAsync(movieGuid, vres.SecureUrl, vres.PublicId, "transcoded", "hls");
                            try
                            {
                                var scopedNotification = scope.ServiceProvider.GetRequiredService<INotificationService>();
                                var movieDetails = await scopedStore.GetByGuidAsync(movieGuid) as IDictionary<string, object>;
                                string movieName = movieDetails != null && movieDetails.TryGetValue("MovieName", out var name) ? name?.ToString() ?? "New Movie" : "New Movie";
                                await scopedNotification.BroadcastNotificationAsync("New Movie Uploaded!", $"'{movieName}' is now available to stream in high quality!", "system");
                            }
                            catch { }
                        }
                    }
                    catch
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var scopedStore = scope.ServiceProvider.GetRequiredService<IMovieStore>();
                            await scopedStore.UpdateMovieVideoMetaAsync(movieGuid, string.Empty, string.Empty, "failed", null);
                        }
                    }
                    finally
                    {
                        videoStream.Dispose();
                    }
                });
            }

            return movieGuid ?? string.Empty;
        }

        public Task<object?> GetByGuidAsync(string movieGuid)
        {
            return _store.GetByGuidAsync(movieGuid);
        }

        public async Task<object?> UploadMovieVideoAsync(string movieGuid, FileUploadDto file)
        {
            if (file == null) return null;

            var persisted = await _store.UpdateMovieVideoMetaAsync(movieGuid, string.Empty, string.Empty, "processing", null);
            if (!persisted) return null;

            var videoStream = new System.IO.MemoryStream();
            await file.Content.CopyToAsync(videoStream);
            videoStream.Position = 0;

            var videoFileDto = new FileUploadDto
            {
                FileName = file.FileName,
                ContentType = file.ContentType,
                Content = videoStream
            };

            await _backgroundTaskQueue.QueueBackgroundWorkItemAsync(async token =>
            {
                try
                {
                    var vres = await _cloudinaryService.UploadVideoAsync(videoFileDto, "streamvault/movies", token);
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var scopedStore = scope.ServiceProvider.GetRequiredService<IMovieStore>();
                        await scopedStore.UpdateMovieVideoMetaAsync(movieGuid, vres.SecureUrl, vres.PublicId, "transcoded", "hls");
                        try
                        {
                            var scopedNotification = scope.ServiceProvider.GetRequiredService<INotificationService>();
                            var movieDetails = await scopedStore.GetByGuidAsync(movieGuid) as IDictionary<string, object>;
                            string movieName = movieDetails != null && movieDetails.TryGetValue("MovieName", out var name) ? name?.ToString() ?? "New Movie" : "New Movie";
                            await scopedNotification.BroadcastNotificationAsync("New Movie Uploaded!", $"'{movieName}' is now available to stream in high quality!", "system");
                        }
                        catch { }
                    }
                }
                catch
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var scopedStore = scope.ServiceProvider.GetRequiredService<IMovieStore>();
                        await scopedStore.UpdateMovieVideoMetaAsync(movieGuid, string.Empty, string.Empty, "failed", null);
                    }
                }
                finally
                {
                    videoStream.Dispose();
                }
            });

            return await _store.GetByGuidAsync(movieGuid);
        }

        public async Task<object?> UploadMovieThumbnailAsync(string movieGuid, FileUploadDto file)
        {
            if (file == null) return null;

            var result = await _cloudinaryService.UploadImageAsync(file, "streamvault/movies/thumbnails");

            var persisted = await _store.UpdateMovieThumbnailAsync(movieGuid, result.SecureUrl, result.PublicId);
            if (!persisted) return null;

            return await _store.GetByGuidAsync(movieGuid);
        }

        public async Task<bool> UpdateMovieAsync(
            string movieGuid, 
            UpdateMovieRequest request, 
            string updatedBy, 
            SV.Common.DTOs.FileUploadDto? video = null, 
            SV.Common.DTOs.FileUploadDto? thumbnail = null)
        {
            // 1. Fetch existing movie details
            var existingObj = await _store.GetByGuidAsync(movieGuid);
            if (existingObj == null)
            {
                return false;
            }

            // Extract existing media values safely to avoid resetting them to NULL in database
            string? oldVideoPublicId = null;
            string? oldThumbnailPublicId = null;
            string? videoUrl = null;
            string? videoPublicId = null;
            string? transcodeStatus = null;
            string? availableFormats = null;
            string? thumbnailUrl = null;
            string? thumbnailPublicId = null;

            if (existingObj is IDictionary<string, object> dict)
            {
                oldVideoPublicId = dict.TryGetValue("MovieVideoPublicId", out var vPid) ? vPid?.ToString() : null;
                oldThumbnailPublicId = dict.TryGetValue("ThumbnailPublicId", out var tPid) ? tPid?.ToString() : null;

                videoUrl = dict.TryGetValue("MovieVideoUrl", out var vUrl) ? vUrl?.ToString() : null;
                videoPublicId = oldVideoPublicId;
                transcodeStatus = dict.TryGetValue("VideoTranscodeStatus", out var vStatus) ? vStatus?.ToString() : null;
                availableFormats = dict.TryGetValue("AvailableFormats", out var vFormats) ? vFormats?.ToString() : null;

                thumbnailUrl = dict.TryGetValue("ThumbnailUrl", out var tUrl) ? tUrl?.ToString() : null;
                thumbnailPublicId = oldThumbnailPublicId;
            }

            bool isNewVideoQueued = false;
            System.IO.MemoryStream? videoStream = null;
            FileUploadDto? videoFileDto = null;

            // 2. Handle video upload
            if (video != null)
            {
                videoUrl = null;
                videoPublicId = null;
                transcodeStatus = "processing";
                availableFormats = null;
                isNewVideoQueued = true;

                videoStream = new System.IO.MemoryStream();
                await video.Content.CopyToAsync(videoStream);
                videoStream.Position = 0;

                videoFileDto = new FileUploadDto
                {
                    FileName = video.FileName,
                    ContentType = video.ContentType,
                    Content = videoStream
                };

                // Clean up old video resource from Cloudinary
                if (!string.IsNullOrWhiteSpace(oldVideoPublicId))
                {
                    await _cloudinaryService.DeleteResourceAsync(oldVideoPublicId, "video");
                }
            }

            // 3. Handle thumbnail upload
            if (thumbnail != null)
            {
                var tres = await _cloudinaryService.UploadImageAsync(thumbnail, "streamvault/movies/thumbnails");
                thumbnailUrl = tres.SecureUrl;
                thumbnailPublicId = tres.PublicId;

                // Clean up old thumbnail resource from Cloudinary
                if (!string.IsNullOrWhiteSpace(oldThumbnailPublicId))
                {
                    await _cloudinaryService.DeleteResourceAsync(oldThumbnailPublicId, "image");
                }
            }

            // 4. Update movie details in database
            var updateSuccess = await _store.UpdateMovieAsync(
                movieGuid,
                request.GenreGuid,
                request.MovieName,
                request.DurationMinutes,
                request.Language,
                request.Rating,
                updatedBy,
                videoUrl,
                videoPublicId,
                transcodeStatus,
                availableFormats,
                thumbnailUrl,
                thumbnailPublicId
            );

            if (updateSuccess && isNewVideoQueued && videoFileDto != null && videoStream != null)
            {
                await _backgroundTaskQueue.QueueBackgroundWorkItemAsync(async token =>
                {
                    try
                    {
                        var vres = await _cloudinaryService.UploadVideoAsync(videoFileDto, "streamvault/movies", token);
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var scopedStore = scope.ServiceProvider.GetRequiredService<IMovieStore>();
                            await scopedStore.UpdateMovieVideoMetaAsync(movieGuid, vres.SecureUrl, vres.PublicId, "transcoded", "hls");
                            try
                            {
                                var scopedNotification = scope.ServiceProvider.GetRequiredService<INotificationService>();
                                var movieDetails = await scopedStore.GetByGuidAsync(movieGuid) as IDictionary<string, object>;
                                string movieName = movieDetails != null && movieDetails.TryGetValue("MovieName", out var name) ? name?.ToString() ?? "New Movie" : "New Movie";
                                await scopedNotification.BroadcastNotificationAsync("New Movie Uploaded!", $"'{movieName}' is now available to stream in high quality!", "system");
                            }
                            catch { }
                        }
                    }
                    catch
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var scopedStore = scope.ServiceProvider.GetRequiredService<IMovieStore>();
                            await scopedStore.UpdateMovieVideoMetaAsync(movieGuid, string.Empty, string.Empty, "failed", null);
                        }
                    }
                    finally
                    {
                        videoStream.Dispose();
                    }
                });
            }

            return updateSuccess;
        }

        public Task<PagedResult<MovieResponseDto>> GetByGenresPagedAsync(int pageNumber, int pageSize, string[]? genres = null)
        {
            return _store.GetByGenresPagedAsync(pageNumber, pageSize, genres);
        }

        public Task<PagedResult<MovieResponseDto>> GetTrendingMoviesPagedAsync(int pageNumber, int pageSize)
        {
            return _store.GetTrendingMoviesPagedAsync(pageNumber, pageSize);
        }

        public Task<List<MovieSuggestResponseDto>> GetSuggestionsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Task.FromResult(new List<MovieSuggestResponseDto>());
            }
            return _store.GetSuggestionsAsync(query);
        }
    }
}
