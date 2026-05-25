using System;
using Newtonsoft.Json;
using Nox.Users;

namespace api.nox.user {
	[Serializable]
	public class UserRelation : IUserRelation {
		[JsonProperty("in")]
		public string In { get; private set; }
		
		[JsonProperty("out")]
		public string Out { get; private set; }
	}
}