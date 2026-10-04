using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nox.Users.Runtime.Networks {
	/// <summary>
	/// Payload of a server socket packet of type <c>event</c>
	/// (e.g. <c>{"name":"user:update","data":{...}}</c>).
	/// </summary>
	internal class ServerEvent {
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("data")]
		public JToken Data { get; set; }
	}
}
