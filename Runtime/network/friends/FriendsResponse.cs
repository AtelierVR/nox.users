using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Nox.Users;

namespace api.nox.user.network {
	[Serializable]
	public class FriendsResponse : ISearchResponse {
		internal string Server;
		internal uint   FetchOffset;
		internal uint   FetchLimit;

		[JsonProperty("items")]
		public User[] Items { get; private set; }

		IUser[] ISearchResponse.Items
			=> Items?.ToArray<IUser>();

		[JsonProperty("total")]
		public uint Total { get; private set; }

		[JsonProperty("limit")]
		public uint Limit { get; private set; }

		[JsonProperty("offset")]
		public uint Offset { get; private set; }

		public bool HasNext()
			=> Offset + Limit < Total;

		public bool HasPrevious()
			=> Offset > 0;

		async UniTask<ISearchResponse> ISearchResponse.Next()
			=> await Next();

		async UniTask<ISearchResponse> ISearchResponse.Previous()
			=> await Previous();

		public async UniTask<FriendsResponse> Next()
			=> HasNext()
				? await Main.Instance.Network.FetchFriends(Offset + Limit, Limit)
				: null;

		public async UniTask<FriendsResponse> Previous()
			=> HasPrevious()
				? await Main.Instance.Network.FetchFriends(Offset - Limit, Limit)
				: null;
	}
}
