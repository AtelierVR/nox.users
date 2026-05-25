using System;

namespace Nox.Users {
	public interface IIntegrityResponse {
		public bool IsError();

		public string GetError();

		public DateTime GetExpires();

		public bool IsExpired();
	}
}