using Cysharp.Threading.Tasks;
using Nox.CCK.Language;
using Nox.CCK.Mods.Cores;
using Nox.CCK.Mods.Initializers;
using Nox.CCK.Network;
using Nox.CCK.Utils;
using Nox.Network;
using Nox.Search;
using Nox.Servers;
using Nox.Tables;
using Nox.Users.Runtime.Networks;
using UnityEngine.Networking;

namespace Nox.Users.Runtime {
	public class Main : IMainModInitializer, IUserAPI {
		static internal Main Instance;
		internal IMainModCoreAPI CoreAPI;
		internal Networks.Network Network;
		private LanguagePack _language;
		private Search.Search _search;

		public static IServerAPI ServerAPI
			=> Instance.CoreAPI.ModAPI
				.GetMod("servers")
				?.GetInstance<IServerAPI>();

		public static INetworkAPI NetworkAPI
			=> Instance.CoreAPI.ModAPI
				.GetMod("network")
				?.GetInstance<INetworkAPI>();

		static internal ISearchAPI SearchAPI
			=> Main.Instance.CoreAPI.ModAPI
				.GetMod("search")
				?.GetInstance<ISearchAPI>();

		static internal ITableAPI TableAPI
			=> Main.Instance.CoreAPI.ModAPI
				.GetMod("tables")
				?.GetInstance<ITableAPI>();

		public async UniTask OnInitializeMainAsync(IMainModCoreAPI api) {
			CoreAPI  = api;
			Instance = this;
			RequestNode.OnCreated.AddListener(OnBeforeRequest);
			Network   = new Networks.Network();
			_language = api.AssetAPI.GetAsset<LanguagePack>("lang.asset");
			if (_language != null) LanguageManager.AddPack(_language);

			_search = new Search.Search();

			var user = Network.CurrentUser;
			user ??= await Network.FetchCurrent();

			if (user == null)
				Logger.LogDebug("User not found");
			else
				Logger.LogDebug("User found: " + user.Username);
		}

		private async UniTask OnBeforeRequest(string address, UnityWebRequest request) {
			var token = await GetToken(address);
			if (token != null)
				request.SetRequestHeader("Authorization", token.ToHeader());

			var uid = Current?.Identifier.ToString();
			if (!string.IsNullOrEmpty(uid))
				request.SetRequestHeader("X-Nox-User", uid);
		}

		public void OnPostInitializeMain() {
			ServerAPI?.OnSocketConnected.AddListener(OnSocketConnected);
			RegisterServerEvents();
		}

		private IServerSocket _registeredSocket;

		/// <summary>
		/// (Re)registers the socket handlers: a fresh <see cref="IServerSocket"/> is created on each
		/// connection, so the handler is attached once per socket instance (handlers persist across
		/// a reconnect of the same socket).
		/// </summary>
		private void OnSocketConnected()
			=> RegisterServerEvents();

		private void RegisterServerEvents() {
			var socket = ServerAPI?.Current;
			if (socket == null || ReferenceEquals(socket, _registeredSocket))
				return;

			_registeredSocket = socket;
			socket.On<ServerEvent>("event", OnServerPacket);
		}

		/// <summary>
		/// Routes the server <c>user:update</c> event: <see cref="Networks.Network.ApplyUserUpdate"/> decides,
		/// from the current user identifier, whether to emit <c>user_update</c> or <c>user_fetch</c>.
		/// </summary>
		private void OnServerPacket(SocketPacket<ServerEvent> packet) {
			if (packet?.payload?.Name != "user:update")
				return;

			Network.ApplyUserUpdate(packet.payload.Data);
		}

		public void OnDisposeMain() {
			ServerAPI?.OnSocketConnected.RemoveListener(OnSocketConnected);
			RequestNode.OnCreated.RemoveListener(OnBeforeRequest);
			_search.Dispose();
			Network.Dispose();
			if (_language != null) LanguageManager.RemovePack(_language);
			_search   = null;
			Network   = null;
			_language = null;
			CoreAPI   = null;
			Instance  = null;
		}

		public ICurrentUser Current
			=> Network.CurrentUser;

		public async UniTask<ICurrentUser> FetchCurrent()
			=> await Network.FetchCurrent();

		public async UniTask<IUser> Fetch(Identifier identifier)
			=> await Network.Fetch(identifier);

		public async UniTask<ISearchResponse> Search(ISearchRequest request, string from = null)
			=> await Network.Search(SearchRequest.FromBase(request), from);

		public async UniTask<ISearchResponse> FetchFriends(uint offset = 0, uint limit = 50)
			=> await Network.FetchFriends(offset, limit);

		public async UniTask<IAuthToken> GetToken(string address)
			=> await Network.GetToken(address);

		public async UniTask<ICurrentUser> UpdateCurrent(IUpdateCurrentUserRequest request)
			=> await Network.UpdateCurrentUser(UpdateCurrentUserRequest.FromBase(request));

		public IUpdateCurrentUserRequest MakeUpdateCurrentRequest()
			=> new UpdateCurrentUserRequest();

		public async UniTask<IFavorites> AddFavorite(Identifier identifier)
			=> await Network.AddFavorite(identifier);

		public async UniTask<IFavorites> RemoveFavorite(Identifier identifier)
			=> await Network.RemoveFavorite(identifier);

		public async UniTask<IFavorites> GetFavorites()
			=> await Network.FetchFavorites();

		public async UniTask<bool> Follow(Identifier identifier)
			=> await Network.Follow(identifier);

		public async UniTask<bool> Unfollow(Identifier identifier)
			=> await Network.Unfollow(identifier);

		public async UniTask<bool> RespondToRequest(Identifier identifier, bool accept)
			=> await Network.RespondToRequest(identifier, accept);
	}
}