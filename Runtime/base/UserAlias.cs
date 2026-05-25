using System;
using Newtonsoft.Json;
using Nox.Users;

namespace api.nox.user {
	[Serializable]
	public class UserAlias : IUserAlias {
		[JsonProperty("key")]
		public string Key { get; }

		[JsonProperty("value")]
		public string Value { get; }
	}
}