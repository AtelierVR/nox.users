using Nox.Users;

namespace Nox.Users.Runtime.Networks {
	public class AuthToken : IAuthToken {
		public bool   Integrity;
		public string Token;

		public string GetToken()
			=> Token;

		public bool IsIntegrity()
			=> Integrity;

		public string ToHeader()
			=> IsIntegrity() ? $"Integrity {Token}" : $"Bearer {Token}";

		public override string ToString()
			=> $"{GetType().Name}[Token={Token}, IsIntegrity={Integrity}]";
	}
}