using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Nox.CCK.Utils;
using Nox.Users;

namespace api.nox.user.network {
	[Serializable]
	public class SearchResponse : ISearchResponse, INoxObject {
		internal SearchRequest Request;
		internal string Server;

		[JsonProperty("items")]
		public User[] Items { get; private set; }

		IUser[] ISearchResponse.Items
			=> Items.ToArray<IUser>();

		[JsonProperty("total")]
		public uint Total { get; }

		[JsonProperty("limit")]
		public uint Limit { get; }

		[JsonProperty("offset")]
		public uint Offset { get; }

		public bool HasNext()
			=> Offset + Limit < Total;

		public bool HasPrevious()
			=> Offset > 0;

		async UniTask<ISearchResponse> ISearchResponse.Next()
			=> await Next();

		async UniTask<ISearchResponse> ISearchResponse.Previous()
			=> await Previous();

		public async UniTask<SearchResponse> Next()
			=> HasNext()
				? await Main.Instance.Network.Search(
					new SearchRequest {
						query  = Request.GetQuery(),
						ids    = Request.GetIds(),
						offset = Offset + Limit,
						limit  = Limit
					},
					Server
				)
				: null;

		public async UniTask<SearchResponse> Previous()
			=> HasPrevious()
				? await Main.Instance.Network.Search(
					new SearchRequest {
						query  = Request.GetQuery(),
						ids    = Request.GetIds(),
						offset = Offset - Limit,
						limit  = Limit
					},
					Server
				)
				: null;
	}
}