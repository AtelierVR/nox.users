using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nox.CCK.Convertors;
using Nox.CCK.Network;
using Nox.CCK.Utils;
using Nox.Users;
using Nox.Users.Runtime;
using Nox.Users.Runtime.Base;
using Nox.Users.Runtime.Networks;
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
			users.Server  = address;
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
			CurrentUser   = null;
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

			var path    = $"/users/@me/friends?offset={offset}&limit={limit}";
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

			foreach (var user in friends.Items ?? System.Array.Empty<User>())
				InvokeFetch(user);

			return friends;
		}

		public void Dispose() {
			CurrentUser = null;
		}

		[Serializable]
		internal class LogoutResponse {
			public bool success;
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
			var login    = response.Data;

			Logger.LogDebug($"Login response: {response.HasError()} {response.Error} {form.ToJson()}");
			if (response.HasError()) {
				var errorInfo = response.Error;
				return new LoginResponse {
					Error = errorInfo.Message,
					Verification = new VerificationRequired {
						Required = errorInfo.Code == "VERIFICATION_REQUIRED",
						Methods  = login?.methods ?? Array.Empty<VerificationMethod>()
					}
				};
			}


			// Successful login - set current user and save config
			CurrentUser   = login.user;
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

		public async UniTask<AuthToken> GetToken(string server) {

			if (string.IsNullOrEmpty(server)) {
				Logger.LogError("Cannot get token: no server provided.");
				return null;
			}

			var address = ServerAddress;
			if (string.IsNullOrEmpty(address)) {
				Logger.LogError("Cannot get token: no server address provided.");
				return null;
			}

			var config = Config.Load();

			if (server == address) {
				if (!config.Has(new[] { "servers", address, "_token" }))
					return null;
				var expires = config.Get(new[] { "servers", address, "expires" }, long.MinValue);
				if (expires > DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
					return new AuthToken {
						Token     = config.Get<string>(new[] { "servers", address, "_token" }),
						Integrity = false
					};
			}

			return null;

		}

		public async UniTask<SendVerificationCodeResponse> SendVerificationCode(string type, string from = null, CancellationToken cancellationToken = default) {

			var address = from ?? CurrentUser?.Server ?? ServerAddress;
			if (string.IsNullOrEmpty(address)) {
				Logger.LogError("Cannot send verification code: no server address provided.");
				return new SendVerificationCodeResponse { success = false, message = "No server address provided." };
			}

			var request = await RequestNode.To(address, $"/auth/{type}/send");
			if (request == null) {
				Logger.LogError($"Failed to create request for verification code");
				return new SendVerificationCodeResponse { success = false, message = "Failed to create request" };
			}

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
			var key   = $"{(pub ? "public." : "")}favorites.users.{group}";
			var entry = await Main.TableAPI.Get(key);
			if (entry == null)
				return new Favorites {
					Key    = key,
					Label  = null,
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
				JsonConvert.SerializeObject(e)
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