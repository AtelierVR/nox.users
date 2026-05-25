using System;
using Newtonsoft.Json;
using Nox.Users;

namespace Nox.Users.Runtime.Base {
	[Serializable]
	public class LinkEntry : ILinkEntry {
		[JsonProperty("label")]
		public string Label { get; }
		
		[JsonProperty("value")]
		public string Value { get; }
	}
}