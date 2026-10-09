using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Nox.Users.Runtime.Base;

namespace Nox.Users.Runtime.Networks {
	[Serializable]
	public class FriendsResponse : ISearchResponse {
		internal string Server;
		internal uint   FetchOffset;
		internal uint   FetchLimit;

		/// <summary>
		/// Raw relation entries returned by <c>GET /users/@me/friends</c>.
		/// The API returns follow relations (the "out" relation from the current user and the
		/// "in" relation from the friend), not user objects, so the friends have to be resolved
		/// into users before being exposed through <see cref="Items"/> (see
		/// <see cref="Network.FetchFriends"/>).
		/// </summary>
		[JsonProperty("items")]
		internal FriendRelation[] Relations { get; private set; }

		/// <summary>
		/// Resolved friend users. Populated by <see cref="Network.FetchFriends"/> once the
		/// relations have been resolved.
		/// </summary>
		public User[] Items { get; internal set; } = Array.Empty<User>();

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

		/// <summary>
		/// A single friend entry: the relation from the current user to the friend (<see cref="Out"/>)
		/// and the relation from the friend to the current user (<see cref="In"/>).
		/// </summary>
		[Serializable]
		internal class FriendRelation {
			[JsonProperty("out")]
			public RelationEntry Out { get; private set; }

			[JsonProperty("in")]
			public RelationEntry In { get; private set; }

			/// <summary>
			/// Identifier of the friend (the other side of the relation).
			/// Prefers the target of the current user's relation, then the initiator.
			/// </summary>
			public string Friend
				=> !string.IsNullOrEmpty(Out?.Target)
					? Out.Target
					: In?.Initiator;
		}

		[Serializable]
		internal class RelationEntry {
			[JsonProperty("type")]
			public string Type { get; private set; }

			[JsonProperty("initiator")]
			public string Initiator { get; private set; }

			[JsonProperty("target")]
			public string Target { get; private set; }
		}
	}
}
