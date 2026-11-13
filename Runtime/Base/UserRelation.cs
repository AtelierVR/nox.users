using System;
using Newtonsoft.Json;

namespace Nox.Users.Runtime.Base {
	[Serializable]
	public class UserRelation : IUserRelation {
		[JsonProperty("in"), JsonConverter(typeof(UserRelationTypeConverter))]
		public UserRelationType In { get; internal set; }

		[JsonProperty("out"), JsonConverter(typeof(UserRelationTypeConverter))]
		public UserRelationType Out { get; internal set; }
	}
}