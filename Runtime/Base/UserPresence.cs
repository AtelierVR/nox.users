using System;
using Newtonsoft.Json;

namespace Nox.Users.Runtime.Base {
	[Serializable]
	public class UserPresence : IUserPresence {
		[JsonProperty("status")]
		public UserStatus Status { get; internal set; }

		[JsonProperty("text")]
		public string Text { get; internal set; }

		[JsonProperty("locations")]
		public string[] Locations { get; internal set; }
	}
}
