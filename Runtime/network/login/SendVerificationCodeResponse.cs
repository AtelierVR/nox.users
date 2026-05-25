using System;
using Nox.CCK.Utils;

namespace api.nox.user.network {
	[Serializable]
	public class SendVerificationCodeResponse : INoxObject {
		public bool success;
		public string message;
		public string method;

		public bool IsSuccess()
			=> success;

		public string GetMessage()
			=> message;

		public string GetMethod()
			=> method;

		public override string ToString()
			=> $"{GetType().Name}[success={success}, message={message}, method={method}]";
	}
}
