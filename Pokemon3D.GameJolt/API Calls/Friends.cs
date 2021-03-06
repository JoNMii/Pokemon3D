﻿using System.Collections.Generic;

namespace Pokemon3D.GameJolt
{
    public partial class API
    {
        public static partial class Calls
        {
            /// <summary>
            /// Contains static methods to create valid API calls to the Game Jolt API to work with friendlists.
            /// </summary>
            public static class Friends
            {
                /// <summary>
                /// Creates an API call that returns all sent friend requests for a user.
                /// </summary>
                public static APICall FetchSentFriendRequests(string username, string token)
                {
                    var parameters = new Dictionary<string, string>();
                    parameters.Add("username", username);
                    parameters.Add("user_token", token);
                    return new APICall("friends/sent-requests", parameters);
                }

                /// <summary>
                /// Creates an API call that returns all received friend requests for a user.
                /// </summary>
                public static APICall FetchReceivedFriendRequests(string username, string token)
                {
                    var parameters = new Dictionary<string, string>();
                    parameters.Add("username", username);
                    parameters.Add("user_token", token);
                    return new APICall("friends/received-requests", parameters);
                }

                /// <summary>
                /// Creates an API call that returns the friend list for a user.
                /// </summary>
                /// <param name="userId">The user id of the owner of the friend list.</param>
                public static APICall FetchFriendList(string userId)
                {
                    var parameters = new Dictionary<string, string>();
                    parameters.Add("user_id", userId);
                    return new APICall("friends", parameters);
                }

                /// <summary>
                /// Creates an API call that sends a friend request to another user.
                /// </summary>
                /// <param name="targetUserId">The target user id of the friend request.</param>
                public static APICall SendFriendRequest(string targetUserId, string username, string token)
                {
                    var parameters = new Dictionary<string, string>();
                    parameters.Add("target_user_id", targetUserId);
                    parameters.Add("username", username);
                    parameters.Add("user_token", token);
                    return new APICall("friends/send-request", parameters);
                }

                /// <summary>
                /// Creates an API call that cancels a friend request to another user.
                /// </summary>
                /// <param name="targetUserId">The target user id of the friend request.</param>
                public static APICall CancelFriendRequest(string targetUserId, string username, string token)
                {
                    var parameters = new Dictionary<string, string>();
                    parameters.Add("target_user_id", targetUserId);
                    parameters.Add("username", username);
                    parameters.Add("user_token", token);
                    return new APICall("friends/cancel-request", parameters);
                }

                /// <summary>
                /// Creates an API call that accepts a friend request from another user.
                /// </summary>
                /// <param name="targetUserId">The sender user id of the friend request.</param>
                public static APICall AcceptFriendRequest(string targetUserId, string username, string token)
                {
                    var parameters = new Dictionary<string, string>();
                    parameters.Add("target_user_id", targetUserId);
                    parameters.Add("username", username);
                    parameters.Add("user_token", token);
                    return new APICall("friends/accept-request", parameters);
                }

                /// <summary>
                /// Creates an API call that declines a friend request from another user.
                /// </summary>
                /// <param name="targetUserId">The sender user id of the friend request.</param>
                public static APICall DeclineFriendRequest(string targetUserId, string username, string token)
                {
                    var parameters = new Dictionary<string, string>();
                    parameters.Add("target_user_id", targetUserId);
                    parameters.Add("username", username);
                    parameters.Add("user_token", token);
                    return new APICall("friends/decline-request", parameters);
                }
            }
        }
    }
}
