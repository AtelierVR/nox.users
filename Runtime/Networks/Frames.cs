using System;
using Newtonsoft.Json;
using Nox.CCK.Convertors;
using Nox.CCK.Utils;
using Nox.Users.Runtime.Base;

namespace Nox.Users.Runtime.Networks {
	[Serializable]
	public class UserChangedFrame {
		[JsonProperty("user"), JsonConverter(typeof(StringToIdentifierConverter))]
		public Identifier User { get; set; }

		[JsonProperty("out")]
		public bool Out { get; set; }

		[JsonProperty("in")]
		public bool In { get; set; }

		[JsonProperty("entity")]
		public User Entity { get; set; }
	}

	[Serializable]
	public class UserRelationFrame {
		[JsonProperty("user"), JsonConverter(typeof(StringToIdentifierConverter))]
		public Identifier User { get; set; }

		[JsonProperty("out"), JsonConverter(typeof(UserRelationTypeConverter))]
		public UserRelationType Out { get; set; }

		[JsonProperty("in"), JsonConverter(typeof(UserRelationTypeConverter))]
		public UserRelationType In { get; set; }
	}

	[Serializable]
	public class UserPresenceFrame {
		[JsonProperty("user"), JsonConverter(typeof(StringToIdentifierConverter))]
		public Identifier User { get; set; }

		[JsonProperty("status"), JsonConverter(typeof(UserStatusConverter))]
		public UserStatus Status { get; set; }

		[JsonProperty("text")]
		public string Text { get; set; }

		[JsonProperty("locations")]
		public string[] Locations { get; set; }
	}

	public class UserStatusConverter : JsonConverter<UserStatus> {
		public override void WriteJson(JsonWriter writer, UserStatus value, JsonSerializer serializer)
			=> writer.WriteValue(value.ToString());

		public override UserStatus ReadJson(
			JsonReader reader,
			Type objectType,
			UserStatus existingValue,
			bool hasExistingValue,
			JsonSerializer serializer
		)
			=> reader.TokenType switch {
				JsonToken.Null or JsonToken.Undefined => UserStatus.NONE,
				JsonToken.String => UserStatusExtensions.FromString((reader.Value as string)?.ToLowerInvariant()),
				_ => throw new JsonSerializationException($"Cannot convert {reader.TokenType} to {nameof(UserStatus)}"),
			};
	}

	[Serializable]
	public class RelationChangedFrame {
		[JsonProperty("target"), JsonConverter(typeof(StringToIdentifierConverter))]
		public Identifier Target { get; set; }

		[JsonProperty("type"), JsonConverter(typeof(RelationActionConverter))]
		public RelationAction Action { get; set; }
	}

	public enum RelationAction {
		None,
		Follow,
		Request,
		Unfollow
	}

	public class RelationActionConverter : JsonConverter<RelationAction> {
		public override void WriteJson(JsonWriter writer, RelationAction value, JsonSerializer serializer)
			=> writer.WriteValue(value.ToString().ToLowerInvariant());

		public override RelationAction ReadJson(
			JsonReader reader,
			Type objectType,
			RelationAction existingValue,
			bool hasExistingValue,
			JsonSerializer serializer
		) {
			switch (reader.TokenType) {
				case JsonToken.Null:
				case JsonToken.Undefined:
					return RelationAction.None;
				case JsonToken.String:
					return (reader.Value as string)?.ToLowerInvariant() switch {
						"follow" => RelationAction.Follow,
						"request" => RelationAction.Request,
						"unfollow" => RelationAction.Unfollow,
						null or "" => RelationAction.None,
						var other => throw new JsonSerializationException($"Unknown relation action '{other}'.")
					};
				default:
					throw new JsonSerializationException($"Cannot convert {reader.TokenType} to {nameof(RelationAction)}");
			}
		}
	}
}
