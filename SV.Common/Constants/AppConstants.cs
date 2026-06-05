using System;
using System.Collections.Generic;
using System.Text;

namespace SV.Common.Constants
{
    public static class AppConstants
    {
        public const string SpLoginUser = "usp_LoginUser";
        public const string SpInsertUser = "usp_InsertUser";

        public const string SpInsertRole = "usp_InsertRole";
        public const string SpUpdateRole = "usp_UpdateRole";
        public const string SpDeleteRole = "usp_DeleteRole";

        public const string SpInsertPlan = "usp_InsertPlan";
        public const string SpUpdatePlan = "usp_UpdatePlan";
        public const string SpDeletePlan = "usp_DeletePlan";

        public const string SpInsertGenre = "usp_InsertGenre";
        public const string SpGetGenres = "GetGenres";

        public const string SpInsertMovie = "usp_InsertMovie";
        public const string SpGetLatestMovies = "usp_GetLatestMovies";

        public const string SpInsertSubscription = "usp_CreateSubscription";
        public const string SpGetUserSubscriptionDetails = "vw_UserSubscriptionDetails"; // view

        public const string SpInsertWatchHistory = "usp_InsertWatchHistory";
        public const string SpGetWatchHistoryPaged = "usp_GetWatchHistoryPaged";

        public const string SpInsertPayment = "usp_InsertPaymentTransaction";

        public const string SpAddToWatchlist = "usp_AddToWatchlist";
        public const string SpRemoveFromWatchlist = "usp_RemoveFromWatchlist";
        public const string SpGetUserWatchlist = "usp_GetUserWatchlist";

        public const string SpAddReview = "usp_AddReview";
        public const string SpGetMovieReviews = "usp_GetMovieReviews";
    }
}
