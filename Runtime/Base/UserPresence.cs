using System;
using Newtonsoft.Json;
using Nox.Users;

namespace Nox.Users.Runtime.Base {
	[Serializable]
	public class UserPresence : IUserPresence {
		[JsonProperty("status")]
		public UserStatus Status { get; }

		[JsonProperty("text")]
		public string Text { get; }
	}
}