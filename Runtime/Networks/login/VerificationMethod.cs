using System;
using Nox.CCK.Utils;

namespace Nox.Users.Runtime.Networks {
	[Serializable]
	// ReSharper disable InconsistentNaming
	public class VerificationMethod : INoxObject {
		public string type;
		public string name;
		public string description;
		public bool enabled;
		public VerificationMethodDetails details;

		public string GetId()
			=> type;

		public string GetTitle()
			=> name;

		public bool IsEnabled()
			=> enabled;

		public string GetDescription()
			=> description;

		public bool CanSend()
			=> details?.sendable ?? false;

		public int GetCodeLength()
			=> details?.code?.length ?? 6;

		public bool IsTotp()
			=> type == "totp";

		public bool IsEmail()
			=> type == "email";

		public override string ToString()
			=> $"{GetType().Name}[type={type}, name={name}, enabled={enabled}]";
	}

	[Serializable]
	public class VerificationMethodDetails {
		public bool sendable;
		public VerificationMethodData data;
		public VerificationMethodCode code;
	}

	[Serializable]
	public class VerificationMethodData {
		public int target;
	}

	[Serializable]
	public class VerificationMethodCode {
		public int length;
		public string type;
	}
}