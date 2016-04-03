using Mattermost.Models;
using Mattermost.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Mattermost
{
    static class MattermostAPI
    {
        public static Uri APIBaseURL { get; private set; }
        public static string Token { get; private set; }
        public static string MyID { get; private set; }
        public static User Me
        {
            get
            {
                return User.GetUserByID(MyID);
            }
        }
        public static Team Team { get; private set; }

        static HttpClient client = new HttpClient();
        static Dictionary<string, object> avatars = new Dictionary<string, object>();

        static async Task<APIResponse<T>> MakeAPIRequest<T>(string method, dynamic postData)
        {
            return await MakeAPIRequest<T>(method, JsonConvert.SerializeObject(postData));
        }

        static async Task<APIResponse<T>> MakeAPIRequest<T>(string method, string postData)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, new Uri(APIBaseURL, method));

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                request.Content = new StringContent(postData, Encoding.UTF8, "application/json");

                bool rateLimited = false;
                HttpResponseMessage response = null;

                while (!rateLimited)
                {
                    response = await client.SendAsync(request);

                    if ((int)response.StatusCode == 429)
                        await Task.Delay(1000);
                    else
                        rateLimited = true;
                }

                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    APIErrorResponse error = JsonConvert.DeserializeObject<APIErrorResponse>(responseContent);

                    return new APIResponse<T>() { Success = false, Error = error.Message };
                }

                return new APIResponse<T>() { Success = true, Value = JsonConvert.DeserializeObject<T>(responseContent) };
            }
            catch (Exception e)
            {
                return new APIResponse<T>() { Success = false, Error = e.Message };
            }
        }

        static async Task<APIResponse<T>> MakeAPIRequest<T>(string method)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, new Uri(APIBaseURL, method));

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                bool rateLimited = false;
                HttpResponseMessage response = null;

                while (!rateLimited)
                {
                    response = await client.SendAsync(request);

                    if ((int)response.StatusCode == 429)
                        await Task.Delay(1000);
                    else
                        rateLimited = true;
                }

                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    APIErrorResponse error = JsonConvert.DeserializeObject<APIErrorResponse>(responseContent);

                    return new APIResponse<T>() { Success = false, Error = error.Message };
                }

                return new APIResponse<T>() { Success = true, Value = JsonConvert.DeserializeObject<T>(responseContent) };
            }
            catch (Exception e)
            {
                return new APIResponse<T>() { Success = false, Error = e.Message };
            }
        }

        static async Task<APIResponse> MakeAPIRequest(string method, dynamic postData)
        {
            return await MakeAPIRequest(method, JsonConvert.SerializeObject(postData));
        }

        static async Task<APIResponse> MakeAPIRequest(string method, string postData)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, new Uri(APIBaseURL, method));

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                request.Content = new StringContent(postData, Encoding.UTF8, "application/json");

                bool rateLimited = false;
                HttpResponseMessage response = null;

                while (!rateLimited)
                {
                    response = await client.SendAsync(request);

                    if ((int)response.StatusCode == 429)
                        await Task.Delay(1000);
                    else
                        rateLimited = true;
                }

                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    APIErrorResponse error = JsonConvert.DeserializeObject<APIErrorResponse>(responseContent);

                    return new APIResponse() { Success = false, Error = error.Message };
                }

                return new APIResponse() { Success = true };
            }
            catch (Exception e)
            {
                return new APIResponse() { Success = false, Error = e.Message };
            }
        }

        static async Task<APIResponse> MakeAPIRequest(string method)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, new Uri(APIBaseURL, method));

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                bool rateLimited = false;
                HttpResponseMessage response = null;

                while (!rateLimited)
                {
                    response = await client.SendAsync(request);

                    if ((int)response.StatusCode == 429)
                        await Task.Delay(1000);
                    else
                        rateLimited = true;
                }

                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    APIErrorResponse error = JsonConvert.DeserializeObject<APIErrorResponse>(responseContent);

                    return new APIResponse() { Success = false, Error = error.Message };
                }

                return new APIResponse() { Success = true };
            }
            catch (Exception e)
            {
                return new APIResponse() { Success = false, Error = e.Message };
            }
        }

        public static async Task<APIResponse> CheckSession(LocalConfig config)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, new Uri(config.Server + "/api/v1/users/me"));

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.Token);

                bool rateLimited = false;
                HttpResponseMessage response = null;

                while (!rateLimited)
                {
                    response = await client.SendAsync(request);

                    if ((int)response.StatusCode == 429)
                        await Task.Delay(1000);
                    else
                        rateLimited = true;
                }

                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    APIErrorResponse error = JsonConvert.DeserializeObject<APIErrorResponse>(responseContent);

                    return new APIResponse() { Success = false, Error = error.Message };
                }

                APIBaseURL = new Uri(config.Server + "/api/v1/");
                Token = config.Token;
                MyID = config.UserID;
                Team = LocalStorage.GetByID<Team>("teams", config.TeamID);

                return new APIResponse() { Success = true };
            }
            catch (Exception e)
            {
                return new APIResponse() { Success = false, Error = e.Message };
            }
        }

        public static async Task<APIResponse> Login(string server, string team, string username, SecureString password)
        {
            APIBaseURL = new Uri(server + "/api/v1/");

            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, new Uri(APIBaseURL, "users/login"));
                Dictionary<string, object> postData = new Dictionary<string, object>() { { "name", team }, { "password", ConvertToString(password) } };

                if (Regex.IsMatch(username, @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?"))
                    postData.Add("email", username);
                else
                    postData.Add("username", username);

                request.Content = new StringContent(JsonConvert.SerializeObject(postData), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    APIErrorResponse error = JsonConvert.DeserializeObject<APIErrorResponse>(responseContent);

                    return new APIResponse() { Success = false, Error = error.Message };
                }

                JObject array = JsonConvert.DeserializeObject<JObject>(responseContent);

                MyID = array["id"].ToString();
                Token = response.Headers.GetValues("Token").First();

                APIResponse<Team> teamObj = await MakeAPIRequest<Team>("teams/me");

                if (!teamObj.Success)
                    return new APIResponse() { Success = false, Error = teamObj.Error };

                Team = teamObj.Value;

                return new APIResponse() { Success = true };
            }
            catch (Exception e)
            {
                return new APIResponse() { Success = false, Error = e.Message };
            }
        }

        public static async Task<APIResponse<List<User>>> GetUsers()
        {
            if (string.IsNullOrWhiteSpace(Token))
                return new APIResponse<List<User>>() { Success = false, Error = "Not logged in" };

            APIResponse<JObject> response = await MakeAPIRequest<JObject>("users/profiles");

            if (!response.Success)
                return new APIResponse<List<User>>() { Success = false, Error = response.Error };

            List<User> users = new List<User>();

            foreach (JProperty obj in response.Value.Children())
            {
                users.Add(obj.Value.ToObject<User>());
            }

            return new APIResponse<List<User>>() { Success = true, Value = users };
        }

        public static async Task<APIResponse<List<Channel>>> GetChannels()
        {
            if (string.IsNullOrWhiteSpace(Token))
                return new APIResponse<List<Channel>>() { Success = false, Error = "Not logged in" };

            APIResponse<JObject> response = await MakeAPIRequest<JObject>("channels/");

            if (!response.Success)
                return new APIResponse<List<Channel>>() { Success = false, Error = response.Error };

            List<Channel> channels = response.Value["channels"].ToObject<List<Channel>>();

            return new APIResponse<List<Channel>>() { Success = true, Value = channels };
        }

        public static async Task<APIResponse<Preferences>> GetPreferences()
        {
            if (string.IsNullOrWhiteSpace(Token))
                return new APIResponse<Preferences>() { Success = false, Error = "Not logged in" };

            APIResponse<JArray> response = await MakeAPIRequest<JArray>("preferences/");

            if (!response.Success)
                return new APIResponse<Preferences>() { Success = false, Error = response.Error };

            foreach (JObject obj in response.Value)
            {
                Preferences.Instance.SetPreference((string)obj["category"], (string)obj["name"], (string)obj["value"]);
            }

            return new APIResponse<Preferences>() { Success = true, Value = Preferences.Instance };
        }

        public static async Task<APIResponse<ChannelPosts>> GetPosts(string channel, int offset, int limit)
        {
            if (string.IsNullOrWhiteSpace(Token))
                return new APIResponse<ChannelPosts>() { Success = false, Error = "Not logged in" };

            APIResponse<JObject> response = await MakeAPIRequest<JObject>(string.Format("channels/{0}/posts/{1}/{2}", channel, offset, limit));

            if (!response.Success)
                return new APIResponse<ChannelPosts> { Success = false, Error = response.Error };

            ChannelPosts posts = new ChannelPosts() { Order = response.Value["order"].ToObject<List<string>>(), Posts = new List<Post>() };

            foreach (JProperty post in response.Value["posts"])
            {
                Post newPost = post.Last.ToObject<Post>();

                if (newPost.Type != PostType.Default)
                    newPost.User = User.System;

                posts.Posts.Add(newPost);
            }

            posts.Order.Reverse();

            return new APIResponse<ChannelPosts>() { Success = true, Value = posts };
        }

        public static async Task<APIResponse<Bitmap>> GetAvatar(string userID)
        {
            // If avatars contains the userID key, the download is either in progress, or has completed
            if (avatars.ContainsKey(userID))
            {
                object avatar = avatars[userID];

                // If the object is an APIResponse, return it
                if (avatar is APIResponse<Bitmap>)
                    return (APIResponse<Bitmap>)avatar;
                else
                {
                    // If not, its a WaitHandle, so cast it and await it
                    WaitHandle handle = (WaitHandle)avatar;

                    await handle.WaitOneAsync();

                    return (APIResponse<Bitmap>)avatars[userID];
                }
            }

            EventWaitHandle ewh = new EventWaitHandle(false, EventResetMode.ManualReset);

            avatars.Add(userID, ewh);

            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, new Uri(APIBaseURL, string.Format("users/{0}/image", userID)));

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                bool rateLimited = false;
                HttpResponseMessage response = null;

                while (!rateLimited)
                {
                    response = await client.SendAsync(request);

                    if ((int)response.StatusCode == 429)
                        await Task.Delay(1000);
                    else
                        rateLimited = true;
                }

                APIResponse<Bitmap> retVal;

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    APIErrorResponse error = JsonConvert.DeserializeObject<APIErrorResponse>(responseContent);
                    retVal = new APIResponse<Bitmap>() { Success = false, Error = error.Message };

                    avatars[userID] = retVal;

                    ewh.Set();

                    return retVal;
                }

                Bitmap avatar = new Bitmap(await response.Content.ReadAsStreamAsync());

                retVal = new APIResponse<Bitmap>() { Success = true, Value = avatar.ApplyMask(Mattermost.Properties.Resources.AvatarMask) };

                avatars[userID] = retVal;

                ewh.Set();

                return retVal;
            }
            catch (Exception e)
            {
                APIResponse<Bitmap> retVal = new APIResponse<Bitmap>() { Success = false, Error = e.Message };

                avatars[userID] = retVal;

                ewh.Set();

                return retVal;
            }
        }

        public static async Task<APIResponse> PostMessage(Channel channel, string message)
        {
            if (string.IsNullOrWhiteSpace(Token))
                return new APIResponse() { Success = false, Error = "Not logged in" };

            CreatePost post = new CreatePost()
            {
                Channel = channel,
                User = Me,
                Created = DateTime.UtcNow,
                Message = message
            };

            return await MakeAPIRequest(string.Format("channels/{0}/create", channel.ID), JsonConvert.SerializeObject(post));
        }

        static string ConvertToString(SecureString secPassword)
        {
            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secPassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }

    class APIErrorResponse
    {
        public string Message { get; set; }
        [JsonProperty("detailed_error")]
        public string DetailedError { get; set; }
        [JsonProperty("request_id")]
        public string RequestID { get; set; }
        [JsonProperty("status_code")]
        public HttpStatusCode StatusCode { get; set; }
    }

    class APIResponse<T>
    {
        public bool Success { get; set; }
        public T Value { get; set; }
        public string Error { get; set; }
    }

    class APIResponse
    {
        public bool Success { get; set; }
        public string Error { get; set; }
    }
}
