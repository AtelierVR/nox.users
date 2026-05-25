using System;

namespace Nox.Users {
	public interface ILoginResponse {
		public bool IsError();

		public string GetError();

		public string GetToken();

		public DateTime GetExpires();

		public ICurrentUser GetUser();

		public bool IsExpired();
	}
}