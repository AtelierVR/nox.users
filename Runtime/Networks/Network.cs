using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nox.CCK.Convertors;
using Nox.CCK.Network;
using Nox.CCK.Utils;
using Nox.Users.Runtime.Base;
using UnityEngine.Events;
using static Nox.CCK.Network.RequestExtension;
using Logger = Nox.CCK.Utils.Logger;

namespace Nox.Users.Runtime.Networks {
	public class Network {
		public CurrentUser CurrentUser;

		public static string ServerAddress {
			get => Config.Load().Get<string>("server");
			set {
				var config = Config.Load();
				config.Set("server", value);
				config.Save();
			}
		}

		public async UniTask<CurrentUser> FetchCurrent(CancellationToken cancellationToken = default) {
			var address = ServerAddress;
			if (string.IsNullOrEmpty(address)) {
				Logger.LogError("Cannot fetch current user: no server address provided.");
				return null;
			}

			var request = await RequestNode.To(address, "/users/@me");
			if (request == null) {
				Logger.LogError($"Failed to create request for current user");
				return null;
			}

			await request.Send(cancellationToken);

			var response = await request.Node<CurrentUser>(cancellationToken);
			if (response.HasError() || !response.HasData()) {
				Logger.LogError($"Failed to fetch current user from {address}: {response.Error?.Message ?? "No data returned"}");
				return null;
			}

			CurrentUser = response.Data;
			InvokeUpdate(CurrentUser);
			return CurrentUser;
		}

		private readonly UnityEvent<User> _fetchEvent = new();
		private readonly UnityEvent<CurrentUser> _updateEvent = new();
		private readonly UnityEvent<CurrentUser> _logoutEvent = new();
		private readonly UnityEvent<CurrentUser> _loginEvent = new();

		private void InvokeFetch(User user) {
			if (user == null)
				return;
			_fetchEvent.Invoke(user);
			Main.Instance.CoreAPI.EventAPI.Emit("user_fetch", user);
		}

		private void InvokeUpdate(CurrentUser user) {
			_updateEvent.Invoke(user);
			Main.Instance.CoreAPI.EventAPI.Emit("user_update", user);
			InvokeFetch(user);
		}

		public void InvokeLogout(CurrentUser user) {
			_logoutEvent.Invoke(user);
			Main.Instance.CoreAPI.EventAPI.Emit("user_logout", user);
			InvokeUpdate(null);
		}

		public void InvokeLogin(CurrentUser user) {
			_loginEvent.Invoke(user);
			Main.Instance.CoreAPI.EventAPI.Emit("user_login", user);
			InvokeUpdate(user);
		}

		public void ApplyCurrentUser(CurrentUser user) {
			if (user == null || !user.Identifier.IsValid())
				return;

			CurrentUser = user;
			InvokeUpdate(CurrentUser);
		}

		public void ApplyUserChanged(UserChangedFrame frame) {
			if (frame == null)
				return;

			if (frame.Entity != null) {
				frame.Entity.ApplyRelation(
					frame.In ? UserRelationType.FOLLOW : UserRelationType.NONE,
					frame.Out ? UserRelationType.FOLLOW : UserRelationType.NONE
				);

				InvokeFetch(frame.Entity);
				Main.Instance.CoreAPI.EventAPI.Emit("user_changed", frame.Entity);
				return;
			}

			if (!frame.User.IsValid())
				return;

			Main.Instance.CoreAPI.EventAPI.Emit("user_changed", frame.User);
		}

		public void ApplyRelation(UserRelationFrame frame) {
			if (frame == null || !frame.User.IsValid())
				return;

			Main.Instance.CoreAPI.EventAPI.Emit("user_relation_changed", frame.User);
		}

		public void ApplyRelationChanged(RelationChangedFrame frame) {
			if (frame == null || !frame.Target.IsValid())
				return;

			Main.Instance.CoreAPI.EventAPI.Emit("user_relation_changed", frame.Target, frame.Action);
		}

		public void ApplyPresence(UserPresenceFrame frame) {
			if (frame == null || !frame.User.IsValid())
				return;

			if (CurrentUser != null && frame.User.Equals(CurrentUser.Identifier)) {
				CurrentUser.ApplyPresence(frame.Status, frame.Text, frame.Locations);
				InvokeUpdate(CurrentUser);
				return;
			}

			Main.Instance.CoreAPI.EventAPI.Emit(
				"user_presence",
				frame.User,
				frame.Status,
				frame.Text,
				frame.Locations
			);
		}

		public UniTask<User> Fetch(Identifier identifier, string from = null, CancellationToken cancellationToken = default)
			=> Fetch(identifier.ToString(), from, cancellationToken);

		public UniTask<User> Fetch(uint id, string from = null, CancellationToken cancellationToken = default)
			=> Fetch(id.ToString(), from, cancellationToken);

		public async UniTask<User> Fetch(string identifier, string from = null, CancellationToken cancellationToken = default) {
			var ide = Identifier.Parse(identifier);
			if (ide.IsLocal())
				ide = new Identifier(ide.Type, ide.Id, ide.Query, from);
			var address = from ?? CurrentUser?.Server ?? ServerAddress ?? ide.Server;
			if (string.IsNullOrEmpty(address)) {
				Logger.LogError($"Cannot fetch user {identifier}: no server address provided.");
				return null;
			}

			var request = await RequestNode.To(address, $"/users/{ide.ToString()}");
			if (request == null) {
				Logger.LogError($"Failed to create request for user {identifier}");
				return null;
			}

			await request.Send(cancellationToken);
			var response = await request.Node<User>(cancellationToken);
			if (response.HasError()) {
				Logger.LogError($"Failed to fetch user {identifier} from {address}: {response.Error.Message}");
				return null;
			}

			var user = response.Data;
			InvokeFetch(user);
			return user;
		}

		/// <summary>
		/// Follows a user, or sends a follow request when the target requires approval
		/// (<c>POST /users/{id}/follow</c>).
		/// </summary>
		public async UniTask<bool> Follow(Identifier identifier, CancellationToken cancellationToken = default) {
			var address = CurrentUser?.Server ?? ServerAddress;
			if (string.IsNullOrEmpty(address)) {
				Logger.LogError("Cannot follow user: no server address provided.");
				return false;
			}

			var request = await RequestNode.To(address, $"/users/{identifier.ToString(address)}/follow", Method.POST);
			if (request == null) {
				Logger.LogError($"Failed to create request to follow {identifier}");
				return false;
			}

			await request.Send(cancellationToken);
			var response = await request.Node<RelationResponse>(cancellationToken);
			if (response.HasError()) {
				Logger.LogError($"Failed to follow {identifier}: {response.Error?.Message ?? "unknown error"}");
				return false;
			}

			Main.Instance.CoreAPI.EventAPI.Emit("user_relation_changed", identifier);
			return true;
		}

		/// <summary>
		/// Removes an existing follow or cancels a pending follow request
		/// (<c>POST /users/{id}/unfollow</c>).
		/// </summary>
		public async UniTask<bool> Unfollow(Identifier identifier, CancellationToken cancellationToken = default) {
			var address = CurrentUser?.Server ?? ServerAddress;
			if (string.IsNullOrEmpty(address)) {
				Logger.LogError("Cannot unfollow user: no server address provided.");
				return false;
			}

			var request = await RequestNode.To(address, $"/users/{identifier.ToString(address)}/unfollow", Method.POST);
			if (request == null) {
				Logger.LogError($"Failed to create request to unfollow {identifier}");
				return false;
			}

			await request.Send(cancellationToken);
			var response = await request.Node<SuccessResponse>(cancellationToken);
			if (response.HasError()) {
				Logger.LogError($"Failed to unfollow {identifier}: {response.Error?.Message ?? "unknown error"}");
				return false;
			}

			Main.Instance.CoreAPI.EventAPI.Emit("user_relation_changed", identifier);
			return true;
		}

		/// <summary>
		/// Accepts or rejects the pending follow request of <paramref name="identifier"/>
		/// (<c>POST /users/{id}/request</c> with <c>type: accept|reject</c>).
		/// </summary>
		public async UniTask<bool> RespondToRequest(Identifier identifier, bool accept, CancellationToken cancellationToken = default) {
			var address = CurrentUser?.Server ?? ServerAddress;
			if (string.IsNullOrEmpty(address)) {
				Logger.LogError("Cannot respond to the follow request: no server address provided.");
				return false;
			}

			var request = await RequestNode.To(address, $"/users/{identifier.ToString(address)}/request", Method.POST);
			if (request == null) {
				Logger.LogError($"Failed to create request to respond to the follow request of {identifier}");
				return false;
			}

			request.SetBody(JObject.FromObject(new { type = accept ? "accept" : "reject" }));
			await request.Send(cancellationToken);
			var response = await request.Node<RelationResponse>(cancellationToken);
			if (response.HasError()) {
				Logger.LogError($"Failed to {(accept ? "accept" : "reject")} the follow request of {identifier}: {response.Error?.Message ?? "unknown error"}");
				return false;
			}

			Main.Instance.CoreAPI.EventAPI.Emit("user_relation_changed", identifier);
			return true;
		}

		public async UniTask<SearchResponse> Search(SearchRequest data, string from = null, CancellationToken cancellationToken = default) {
			var address = from ?? CurrentUser?.Server ?? ServerAddress;
			if (string.IsNullOrEmpty(address)) {
				Logger.LogError("Cannot search users: no server address provided.");
				return null;
			}

			var request = await RequestNode.To(address, $"/users?{data.ToParams()}");
			if (request == null) {
				Logger.LogError($"Failed to create request for user search");
				return null;
			}

			await request.Send(cancellationToken);
			var response = await request.Node<SearchResponse>(cancellationToken);
			if (response.HasError()) {
				Logger.LogError($"Failed to search users from {address}: {response.Error.Message}");
				return null;
			}

			var users = response.Data;
			users.Server = address;
			users.Request = data;

			foreach (var user in users.Items)
				InvokeFetch(user);

			return users;
		}

		public async UniTask<bool> Logout(CancellationToken cancellationToken = default) {
			if (Main.NetworkAPI == null)
				return false;

			var address = ServerAddress;
			if (string.IsNullOrEmpty(address)) {
				Logger.LogError("Cannot logout: no server address provided.");
				return false;
			}

			var request = await RequestNode.To(address, "/auth/logout", Method.POST);
			if (request == null) {
				Logger.LogError($"Failed to create request for logout");
				return false;
			}

			await request.Send(cancellationToken);
			var response = await request.Node<LogoutResponse>(cancellationToken);
			if (response.HasError()) {
				Logger.LogError($"Failed to logout from {address}: {response.Error.Message}");
				return false;
			}

			var loggedOutUser = CurrentUser;
			CurrentUser = null;
			ServerAddress = null;
			var config = Config.Load();
			config.Remove(new[] { "server", address, "_token" });
			config.Remove(new[] { "server", address, "expires" });
			config.Remove(new[] { "server", address, "user_id" });
			config.Remove(new[] { "server", address, "integrity" });
			config.Save();
			InvokeLogout(loggedOutUser);
			return true;
		}

		public async UniTask<FriendsResponse> FetchFriends(uint offset = 0, uint limit = 50, CancellationToken cancellationToken = default) {
			var address = CurrentUser?.Server ?? ServerAddress;
			if (string.IsNullOrEmpty(address)) {
				Logger.LogError("Cannot fetch friends: no server address provided.");
				return null;
			}

			var path = $"/users/@me/friends?offset={offset}&limit={limit}";
			var request = await RequestNode.To(address, path);
			if (request == null) {
				Logger.LogError("Failed to create request for friends list");
				return null;
			}

			await request.Send(cancellationToken);
			var response = await request.Node<FriendsResponse>(cancellationToken);
			if (response.HasError() || !response.HasData()) {
				Logger.LogError($"Failed to fetch friends from {address}: {response.Error?.Message ?? "No data returned"}");
				return null;
			}

			var friends = response.Data;
			friends.Server = address;

			// The endpoint returns follow relations, not users: resolve the other side of
			// each relation into an actual user before exposing them.
			friends.Items = await ResolveFriends(friends, address, cancellationToken);

			foreach (var user in friends.Items ?? System.Array.Empty<User>())
				InvokeFetch(user);

			return friends;
		}

		/// <summary>
		/// Resolves the friend relations of a <see cref="FriendsResponse"/> into users.
		/// Friends living on another server are resolved against their own server.
		/// </summary>
		private async UniTask<User[]> ResolveFriends(FriendsResponse friends, string address, CancellationToken cancellationToken = default) {
			var relations = friends?.Relations ?? System.Array.Empty<FriendsResponse.FriendRelation>();
			if (relations.Length == 0)
				return System.Array.Empty<User>();

			var identifiers = new List<Identifier>(relations.Length);
			foreach (var relation in relations) {
				var friend = relation?.Friend;
				if (string.IsNullOrEmpty(friend))
					continue;
				var identifier = Identifier.Parse(friend);
				// The API returns bare identifiers (e.g. "2@hactazia.fr"), normalise the type.
				if (string.IsNullOrEmpty(identifier.Type))
					identifier = new Identifier("u", identifier.Id, identifier.Query, identifier.Server);
				if (!identifiers.Contains(identifier))
					identifiers.Add(identifier);
			}

			if (identifiers.Count == 0)
				return Array.Empty<User>();

			var users = new List<User>(identifiers.Count);
			foreach (var group in identifiers.GroupBy(i => i.IsLocal(address) ? address : i.Server)) {
				var server = group.Key;
				if (string.IsNullOrEmpty(server))
					continue;
				var ids = group
					.Select(i => i.IsLocal(address) ? new Identifier(i.Type, i.Id, i.Query, address) : i)
					.ToArray();
				var response = await Search(
					new SearchRequest {
						ids = ids,
						limit = (uint)ids.Length
					},
					server,
					cancellationToken
				);
				if (response?.Items != null)
					users.AddRange(response.Items);
			}

			return users.ToArray();
		}

		public void Dispose() {
			CurrentUser = null;
		}

		[Serializable]
		internal class LogoutResponse {
			public bool success;
		}

		[Serializable]
		internal class SuccessResponse {
			public bool success;
		}

		[Serializable]
		internal class RelationResponse {
			public string type;
			public string initiator;
			public string target;
		}

		public async UniTask<LoginResponse> Login(LoginRequest form, string address, CancellationToken cancellationToken = default) {
			if (string.IsNullOrEmpty(address)) {
				Logger.LogError("Cannot login: no server address provided.");
				return new LoginResponse { Error = "No server address provided." };
			}

			var request = await RequestNode.To(address, "/auth/login", Method.POST);
			if (request == null) {
				Logger.LogError($"Failed to create request for login");
				return new LoginResponse { Error = "Failed to create request" };
			}

			request.SetBody(form.ToJson());
			await request.Send(cancellationToken);

			var response = await request.Node<LoginResponse>(cancellationToken);
			var login = response.Data;

			Logger.LogDebug($"Login response: {response.HasError()} {response.Error} {form.ToJson()}");
			if (response.HasError()) {
				var errorInfo = response.Error;
				return new LoginResponse {
					Error = errorInfo.Message,
					Verification = new VerificationRequired {
						Required = errorInfo.Code == "VERIFICATION_REQUIRED",
						Methods = login?.methods ?? Array.Empty<VerificationMethod>()
					}
				};
			}


			// Successful login - set current user and save config
			CurrentUser = login.user;
			ServerAddress = login.user.Server;
			var config = Config.Load();
			config.Set(new[] { "servers", login.user.Server, "_token" }, login.token);
			config.Set(new[] { "servers", login.user.Server, "expires" }, login.expires);
			config.Set(new[] { "servers", login.user.Server, "user_id" }, login.user.Id);
			config.Remove(new[] { "servers", login.user.Server, "integrity" });
			config.Save();
			InvokeLogin(login.user);
			return login;
		}

		[Obsolete("use your own node server to interact with others servers")]
		public async UniTask<IntegrityResponse> CreateIntegrity(string server, CancellationToken cancellationToken = default) {
			var address = ServerAddress;
			if (string.IsNullOrEmpty(address)) {
				Logger.LogError("Cannot create integrity: no server address provided.");
				return new IntegrityResponse { Error = "No server address provided." };
			}

			var request = await RequestNode.To(address, "/users/@me/integrity");
			if (request == null) {
				Logger.LogError($"Failed to create request for integrity");
				return new IntegrityResponse { Error = "Failed to create request" };
			}

			request.SetBody(new JObject {
				["address"] = server,
			});
			request.method = RequestExtension.Method.PUT;
			await request.Send(cancellationToken);
			var response = await request.Node<IntegrityResponse>(cancellationToken);
			if (response.HasError()) {
				Logger.LogError($"Failed to create integrity from {address} for {server}: {response.Error.Message}");
				return new IntegrityResponse { Error = response.Error.Message };
			}

			var integrity = response.Data;
			if (integrity.IsError()) {
				Logger.LogError($"Integrity creation error from {address} for {server}: {integrity.Error}");
				return integrity;
			}

			var config = Config.Load();
			config.Set(new[] { "servers", address, "integrity", server, "_token" }, integrity.token);
			config.Set(new[] { "servers", address, "integrity", server, "expires" }, integrity.expires);
			config.Save();
			return integrity;
		}

		public UniTask<AuthToken> GetToken(string server) {

			if (string.IsNullOrEmpty(server)) {
				Logger.LogError("Cannot get token: no server provided.");
				return UniTask.FromResult<AuthToken>(null);
			}

			var address = ServerAddress;
			if (string.IsNullOrEmpty(address)) {
				Logger.LogError("Cannot get token: no server address provided.");
				return UniTask.FromResult<AuthToken>(null);
			}

			var config = Config.Load();

			if (server == address) {
				if (!config.Has(new[] { "servers", address, "_token" }))
					return UniTask.FromResult<AuthToken>(null);
				var expires = config.Get(new[] { "servers", address, "expires" }, long.MinValue);
				if (expires > DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
					return UniTask.FromResult(new AuthToken {
						Token = config.Get<string>(new[] { "servers", address, "_token" }),
						Integrity = false
					});
			}

			return UniTask.FromResult<AuthToken>(null);

		}

		public async UniTask<SendVerificationCodeResponse> SendVerificationCode(string type, string from = null, int target = 1, CancellationToken cancellationToken = default) {

			var address = from ?? CurrentUser?.Server ?? ServerAddress;
			if (string.IsNullOrEmpty(address)) {
				Logger.LogError("Cannot send verification code: no server address provided.");
				return new SendVerificationCodeResponse { success = false, message = "No server address provided." };
			}

			var request = await RequestNode.To(address, $"/auth/methods/{type}/send", Method.POST);
			if (request == null) {
				Logger.LogError($"Failed to create request for verification code");
				return new SendVerificationCodeResponse { success = false, message = "Failed to create request" };
			}

			request.SetBody(JObject.FromObject(new { type, target }));
			await request.Send(cancellationToken);
			var response = await request.Node<SendVerificationCodeResponse>(cancellationToken);
			if (response.HasError()) {
				Logger.LogError($"Failed to send verification code from {address}: {response.Error.Message}");
				return new SendVerificationCodeResponse { success = false, message = response.Error.Message };
			}

			var verificationResponse = response.Data;
			Logger.LogDebug($"Verification code send result: {verificationResponse}");
			return verificationResponse;
		}

		public async UniTask<CurrentUser> UpdateCurrentUser(UpdateCurrentUserRequest data, string from = null, CancellationToken cancellationToken = default) {

			var address = from ?? CurrentUser?.Server ?? ServerAddress;
			if (string.IsNullOrEmpty(address)) {
				Logger.LogError("Cannot update current user: no server address provided.");
				return null;
			}

			var request = await RequestNode.To(address, "/users/@me");
			if (request == null) {
				Logger.LogError($"Failed to create request for update current user");
				return null;
			}

			request.SetBody(data.ToJson());
			request.method = RequestExtension.Method.POST;
			await request.Send(cancellationToken);

			var response = await request.Node<CurrentUser>(cancellationToken);
			if (response.HasError()) {
				Logger.LogError($"Failed to update current user: {response.Error.Message}");
				return null;
			}

			var updateResult = response.Data;
			if (updateResult == null) {
				Logger.LogError("Failed to update current user: no data returned.");
				return null;
			}

			CurrentUser = updateResult;
			InvokeUpdate(CurrentUser);
			Logger.LogDebug($"Current user updated: {CurrentUser}");
			return CurrentUser;
		}








		[Serializable]
		public class Favorites : IFavorites {
			[JsonIgnore]
			public string Key { get; set; }
			[JsonProperty("label")]
			public string Label { get; set; }
			[JsonProperty("values"), JsonConverter(typeof(ArrayConverter<StringToIdentifierConverter>))]
#pragma warning disable UAC1001
			public Identifier[] Values { get; set; }
#pragma warning restore UAC1001
		}

		/// <summary>
		/// Fetch favorite users from the specified server
		/// </summary>
		/// <returns></returns>
		public async UniTask<Favorites> FetchFavorites(uint group = 0, bool pub = true) {
			var key = $"{(pub ? "public." : "")}favorites.users.{group}";
			var entry = await Main.TableAPI.Get(key);
			if (entry == null)
				return new Favorites {
					Key = key,
					Label = null,
					Values = Array.Empty<Identifier>()
				};
			var result = JsonConvert.DeserializeObject<Favorites>(entry.AsString);
			result.Key = entry.Key;
			return result;
		}

		/// <summary>
		/// Add a user to favorites on the specified server
		/// </summary>
		/// <param name="identifier"></param>
		/// <param name="group"></param>
		/// <param name="pub"></param>
		/// <returns></returns>
		public async UniTask<Favorites> AddFavorite(Identifier identifier, uint group = 0, bool pub = true)
			=> await AddFavorites(new[] { identifier }, group, pub);

		/// <summary>
		/// Add users to favorites on the specified server
		/// </summary>
		/// <param name="identifier"></param>
		/// <param name="group"></param>
		/// <param name="pub"></param>
		/// <returns></returns>
		public async UniTask<Favorites> AddFavorites(Identifier[] identifier, uint group = 0, bool pub = true) {
			var e = await FetchFavorites(group, pub);
			e.Values = identifier
				.Concat(e.Values)
				.Distinct()
				.ToArray();

			var entry = await Main.TableAPI.Set(
				e.Key,
				JsonConvert.SerializeObject(e),
				"application/json+favorite"
			);

			if (entry != null)
				return null;

			Logger.LogError("Failed to add favorites: entry not found.");
			return e;
		}

		/// <summary>
		/// Remove a world from favorites on the specified server
		/// </summary>
		/// <param name="identifier"></param>
		/// <param name="group"></param>
		/// <param name="pub"></param>
		/// <returns></returns>
		public async UniTask<Favorites> RemoveFavorite(Identifier identifier, uint group = 0, bool pub = true)
			=> await RemoveFavorites(new[] { identifier }, group, pub);

		/// <summary>
		/// Remove users from favorites on the specified server
		/// </summary>
		/// <param name="identifier"></param>
		/// <param name="group"></param>
		/// <param name="pub"></param>
		/// <returns></returns>
		public async UniTask<Favorites> RemoveFavorites(Identifier[] identifier, uint group = 0, bool pub = true) {
			var e = await FetchFavorites(group, pub);
			e.Values = e.Values
				.Where(i => !identifier.Contains(i))
				.ToArray();

			var entry = await Main.TableAPI.Set(
				e.Key,
				JsonConvert.SerializeObject(e)
			);

			if (entry != null)
				return null;

			Logger.LogError($"Failed to add favorites: entry not found.");
			return e;
		}
	}
}
