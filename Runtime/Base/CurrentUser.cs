using System;
using Newtonsoft.Json;
using Nox.CCK.Convertors;
using Nox.CCK.Utils;
using Nox.Users;

namespace Nox.Users.Runtime.Base {
	[Serializable]
	public class CurrentUser : User, ICurrentUser {
		[JsonProperty("email")]
		public string Email { get; private set; }

		[JsonProperty("email_verified")]
		public bool IsEmailVerified { get; private set; }

		[JsonProperty("home"), JsonConverter(typeof(StringToIdentifierConverter))]
		public Identifier Home { get; private set; }

		[JsonProperty("avatar"), JsonConverter(typeof(StringToIdentifierConverter))]
		public Identifier Avatar { get; private set; }

		[JsonProperty("twofa_enabled")]
		public bool Is2FAEnabled { get; private set; }
	}
}