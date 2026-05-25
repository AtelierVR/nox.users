using System;
using Nox.CCK.Utils;

namespace Nox.Users.Runtime.Networks {
	public class VerificationRequired : INoxObject {
		public bool Required;
		public VerificationMethod[] Methods;

		public static VerificationRequired None
			=> new() {
				Required = false,
				Methods  = Array.Empty<VerificationMethod>()
			};
	}
}