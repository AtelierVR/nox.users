using System;
using Nox.Users;

namespace api.nox.user.network {
	[Serializable]
	public class IntegrityResponse : IIntegrityResponse {
		[NonSerialized] public string Error;

		public string token;
		public long   expires;

		public bool IsError()
			=> !string.IsNullOrEmpty(Error);

		public string GetError()
			=> Error;

		public DateTime GetExpires()
			=> DateTimeOffset.FromUnixTimeMilliseconds(expires)
				.UtcDateTime;

		public bool IsExpired()
			=> GetExpires() < DateTime.UtcNow;
	}
}