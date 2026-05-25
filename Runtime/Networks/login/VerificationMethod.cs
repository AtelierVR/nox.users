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
		public bool can_send;

		public string GetId()
			=> type;

		public string GetTitle()
			=> name;

		public bool IsEnabled()
			=> enabled;

		public string GetDescription()
			=> description;

		public bool CanSend()
			=> can_send;

		public bool IsTotp()
			=> type == "totp";

		public bool IsEmail()
			=> type == "email";

		public override string ToString()
			=> $"{GetType().Name}[type={type}, name={name}, enabled={enabled}]";
	}
}