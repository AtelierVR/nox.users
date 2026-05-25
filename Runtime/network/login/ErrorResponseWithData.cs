using System;
using Newtonsoft.Json;
using Nox.CCK.Utils;

namespace api.nox.user.network {
	[Serializable]
	public class ErrorResponseWithData : INoxObject {
		public ErrorInfo error;
		public VerificationMethodsData data;
		public long time;
		public string request;

		public bool IsVerificationRequired()
			=> error is { code: 20 }; // Code 20 = verification required

		public VerificationMethod[] GetVerificationMethods()
			=> data?.methods ?? Array.Empty<VerificationMethod>();

		public string GetMessage()
			=> error?.message ?? "Verification required";

		public override string ToString()
			=> $"{GetType().Name}[error={error}, methods={data?.methods?.Length ?? 0}]";
	}

	[Serializable]
	public class ErrorInfo : INoxObject {
		public int code;
		public string message;
		public int status;

		public override string ToString()
			=> $"{GetType().Name}[code={code}, message={message}, status={status}]";
	}

	[Serializable]
	public class VerificationMethodsData : INoxObject {
		public VerificationMethod[] methods;
	}
}
