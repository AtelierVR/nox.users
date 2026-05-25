using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Nox.CCK.Convertors;
using Nox.CCK.Utils;
using Nox.Users;

namespace Nox.Users.Runtime.Base {
	[Serializable]
	public class User : IUser, INoxObject {
		[JsonProperty("id")]
		public uint Id { get; private set; }

		[JsonProperty("username")]
		public string Username { get; private set; }

		[JsonProperty("display")]
		public string Display { get; private set; }

		[JsonProperty("bio")]
		public string Bio { get; private set; }

		[JsonProperty("pronoun")]
		public string Pronoun { get; private set; }

		[JsonProperty("server")]
		public string Server { get; private set; }

		[JsonProperty("tags")]
		public string[] Tags { get; private set; }

		[JsonProperty("thumbnail")]
		public string Thumbnail { get; private set; }

		[JsonProperty("banner")]
		public string Banner { get; private set; }

		[JsonProperty("links")]
		public LinkEntry[] Links { get; private set; }

		ILinkEntry[] IUser.Links
			=> Links.ToArray<ILinkEntry>();

		[JsonProperty("relations")]
		public UserRelation Relations { get; private set; }

		IUserRelation IUser.Relations
			=> Relations;

		[JsonProperty("public"), JsonConverter(typeof(Base64ToBytes))]
		public byte[] Public { get; private set; }

		[JsonProperty("followers")]
		public int Followers { get; }

		[JsonProperty("following")]
		public int Following { get; }

		[JsonProperty("presence")]
		public UserPresence Presence { get; private set; }

		IUserPresence IUser.Presence
			=> Presence;

		[JsonProperty("alias")]
		public UserAlias[] Aliases { get; private set; }

		IUserAlias[] IUser.Aliases
			=> Aliases.ToArray<IUserAlias>();

		[JsonProperty("created_at"), JsonConverter(typeof(UnixTimestampToDateTime))]
		public DateTime CreatedAt { get; }

		public Identifier Identifier
			=> new("u", Id, null, Server);

		public async UniTask<User> Refresh()
			=> await Main.Instance.Network.Fetch(Identifier, Server);

		async UniTask<IUser> IUser.Refresh()
			=> await Refresh();

		public override string ToString()
			=> $"{GetType().Name}[id={Identifier.ToString()}, username={Username}]";
	}
}