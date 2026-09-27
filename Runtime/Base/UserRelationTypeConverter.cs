using System;
using Newtonsoft.Json;

namespace Nox.Users.Runtime.Base {
	/// <summary>
	/// JSON converter for <see cref="UserRelationType"/>.
	/// <para>
	/// The wire format is a nullable string (<c>string | null</c>):
	/// <c>"follow"</c>, <c>"request"</c> or <c>null</c>.
	/// <c>null</c> (and the absence of the field) is mapped
	/// to <see cref="UserRelationType.NONE"/>, which is written back as <c>null</c>.
	/// </para>
	/// </summary>
	public class UserRelationTypeConverter : JsonConverter<UserRelationType> {
		/// <summary>
		/// Writes the relation as its wire string, or <c>null</c> for <see cref="UserRelationType.NONE"/>.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="serializer"></param>
		public override void WriteJson(JsonWriter writer, UserRelationType value, JsonSerializer serializer) {
			if (value == UserRelationType.NONE) {
				writer.WriteNull();
                return;
            }
			writer.WriteValue(UserRelationTypeExtensions.ToString(value));
		}

		/// <summary>
		/// Reads the relation from a nullable wire string.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="objectType"></param>
		/// <param name="existingValue"></param>
		/// <param name="hasExistingValue"></param>
		/// <param name="serializer"></param>
		/// <returns></returns>
		/// <exception cref="JsonSerializationException"></exception>
		public override UserRelationType ReadJson(JsonReader reader, Type objectType, UserRelationType existingValue, bool hasExistingValue, JsonSerializer serializer)
			=> reader.TokenType switch {
				JsonToken.Null      => UserRelationType.NONE,
				JsonToken.Undefined => UserRelationType.NONE,
				JsonToken.String    => UserRelationTypeExtensions.FromString((string)reader.Value),
				_                   => throw new JsonSerializationException($"Cannot convert {reader.TokenType} to {nameof(UserRelationType)}")
			};
	}
}
