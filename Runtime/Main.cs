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
using Nox.Users.Runtime.Base;
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
			CoreAPI = api;
			Instance = this;
			RequestNode.OnCreated.AddListener(OnBeforeRequest);
			Network = new Networks.Network();
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
			_events = CoreAPI?.ModAPI
				.GetMod("servers")
				?.GetInstance<IServerEventsAPI>();

			if (_events == null) {
				Logger.LogWarning("The servers mod exposes no WebSocket events: the user frames stay off.");
				return;
			}

			_events.On<CurrentUser>("current.changed", OnCurrentUserFrame);
			_events.On<UserChangedFrame>("user.changed", OnUserChangedFrame);
			_events.On<UserRelationFrame>("user.relation", OnUserRelationFrame);
			_events.On<RelationChangedFrame>("relation.changed", OnRelationChangedFrame);
			_events.On<UserPresenceFrame>("user.presence", OnPresenceFrame);
		}

		private IServerEventsAPI _events;

		private void OnCurrentUserFrame(SocketPacket<CurrentUser> packet)
			=> Network.ApplyCurrentUser(packet?.payload);

		private void OnUserChangedFrame(SocketPacket<UserChangedFrame> packet)
			=> Network.ApplyUserChanged(packet?.payload);

		private void OnUserRelationFrame(SocketPacket<UserRelationFrame> packet)
			=> Network.ApplyRelation(packet?.payload);

		private void OnRelationChangedFrame(SocketPacket<RelationChangedFrame> packet)
			=> Network.ApplyRelationChanged(packet?.payload);

		private void OnPresenceFrame(SocketPacket<UserPresenceFrame> packet)
			=> Network.ApplyPresence(packet?.payload);

		public void OnDisposeMain() {
			if (_events != null) {
				_events.Off<CurrentUser>("current.changed", OnCurrentUserFrame);
				_events.Off<UserChangedFrame>("user.changed", OnUserChangedFrame);
				_events.Off<UserRelationFrame>("user.relation", OnUserRelationFrame);
				_events.Off<RelationChangedFrame>("relation.changed", OnRelationChangedFrame);
				_events.Off<UserPresenceFrame>("user.presence", OnPresenceFrame);
			}

			_events = null;
			RequestNode.OnCreated.RemoveListener(OnBeforeRequest);
			_search.Dispose();
			Network.Dispose();
			if (_language != null) LanguageManager.RemovePack(_language);
			_search = null;
			Network = null;
			_language = null;
			CoreAPI = null;
			Instance = null;
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

		public void WatchPresence()
			=> _events?.Subscribe("user.presence");

		public void UnwatchPresence()
			=> _events?.Unsubscribe("user.presence");

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
