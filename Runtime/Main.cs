using api.nox.user.network;
using api.nox.user.search;
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
using Nox.Users;
using UnityEngine.Networking;

namespace api.nox.user {
	public class Main : IMainModInitializer, IUserAPI {
		static internal Main Instance;
		internal IMainModCoreAPI CoreAPI;
		internal Network Network;
		private LanguagePack _language;
		private Search _search;

		public static IServerAPI ServerAPI
			=> Instance.CoreAPI.ModAPI
				.GetMod("server")
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
			Network   = new Network();
			_language = api.AssetAPI.GetAsset<LanguagePack>("lang.asset");
			LanguageManager.AddPack(_language);

			_search = new Search();

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

		public void OnPostInitializeMain() { }

		public void OnDisposeMain() {
			RequestNode.OnCreated.RemoveListener(OnBeforeRequest);
			_search.Dispose();
			Network.Dispose();
			LanguageManager.RemovePack(_language);
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

		public ISearchRequest MakeSearchRequest()
			=> new SearchRequest();

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
			=> (await Network.FetchFavorites());
	}
}