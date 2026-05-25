using System;
using Nox.CCK.Utils;
using Nox.Users;
using Nox.Users.Runtime.Base;

namespace Nox.Users.Runtime.Networks {
	[Serializable]
	public class LoginResponse : ILoginResponse, INoxObject {
		[NonSerialized]
		public string Error;

		[NonSerialized]
		public VerificationRequired Verification = VerificationRequired.None;

		public string token;
		public long expires;
		public CurrentUser user;
		public VerificationMethod[] methods;

		public bool IsError()
			=> !string.IsNullOrEmpty(Error);

		public bool IsVerificationRequired()
			=> IsError() && Verification.Required;

		public string GetError()
			=> Error;

		public string GetToken()
			=> token;

		public VerificationRequired GetVerification()
			=> Verification;

		public DateTime GetExpires()
			=> DateTimeOffset.FromUnixTimeMilliseconds(expires)
				.UtcDateTime;

		public ICurrentUser GetUser()
			=> user;

		public bool IsExpired()
			=> GetExpires() < DateTime.UtcNow;

		public override string ToString()
			=> $"{GetType().Name}[{(IsError() ? $"Error={GetError()}" : $"Token={GetToken()}, Expires={GetExpires()}, User={GetUser()?.ToString() ?? "null"}")}]";
	}
}