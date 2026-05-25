using System;
using Nox.CCK.Utils;
using UnityEngine.Serialization;

namespace api.nox.user.network {
	public class VerificationRequired : INoxObject {
		public bool                 Required;
		public VerificationMethod[] Methods;

		public static VerificationRequired None
			=> new() {
				Required = false,
				Methods  = Array.Empty<VerificationMethod>()
			};
	}
}