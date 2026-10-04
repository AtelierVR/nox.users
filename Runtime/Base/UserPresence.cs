using System;
using Newtonsoft.Json;

namespace Nox.Users.Runtime.Base {
	[Serializable]
	public class UserPresence : IUserPresence {
		[JsonProperty("status")]
		public UserStatus Status { get; private set; }

		[JsonProperty("text")]
		public string Text { get; private set; }
	}
}