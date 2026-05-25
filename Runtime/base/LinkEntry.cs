using System;
using Newtonsoft.Json;
using Nox.Users;

namespace api.nox.user {
	[Serializable]
	public class LinkEntry : ILinkEntry {
		[JsonProperty("label")]
		public string Label { get; }
		
		[JsonProperty("value")]
		public string Value { get; }
	}
}