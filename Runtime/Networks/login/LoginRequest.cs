using System.Linq;
using Newtonsoft.Json.Linq;
using Nox.CCK.Utils;
using Nox.Users;

namespace Nox.Users.Runtime.Networks {
	public class LoginRequest : ILoginRequest, INoxObject {
		internal string Identifier;
		internal string Password;
		internal string FactorCode;
		internal string PublicKey;

		public JObject ToJson() {
			var obj = new JObject {
				["identifier"] = Identifier,
				["password"]   = Password,
			};

			if (!string.IsNullOrEmpty(PublicKey))
				obj["public_key"] = PublicKey;

			if (!string.IsNullOrEmpty(FactorCode))
				obj["factor_code"] = FactorCode;

			return obj;
		}

		public override string ToString()
			=> $"{GetType().Name}[identifier={Identifier}, password={string.Join("", Password.Split().Select(c => '*'))}, factor_code={(!string.IsNullOrEmpty(FactorCode) ? "***" : "null")}]";

		public ILoginRequest SetPassword(string password) {
			Password = password;
			return this;
		}

		public ILoginRequest SetIdentifier(string identifier) {
			Identifier = identifier;
			return this;
		}

		public ILoginRequest SetFactorCode(string factorCode) {
			FactorCode = factorCode;
			return this;
		}

		public string GetPublicKey()
			=> PublicKey;

		public ILoginRequest SetPublicKey(string publicKey) {
			PublicKey = publicKey;
			return this;
		}

		public string GetFactorCode()
			=> FactorCode;

		public string GetPassword()
			=> Password;

		public string GetIdentifier()
			=> Identifier;
	}
}