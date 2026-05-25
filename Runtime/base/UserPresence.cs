using System;
using Newtonsoft.Json;
using Nox.Users;

namespace api.nox.user {
	[Serializable]
	public class UserPresence : IUserPresence {
		[JsonProperty("status")]
		public UserStatus Status { get; }

		[JsonProperty("text")]
		public string Text { get; }
	}
}