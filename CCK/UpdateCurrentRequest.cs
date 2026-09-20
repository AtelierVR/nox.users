using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Nox.Users;

namespace Nox.CCK.Users {
	/// <summary>
	/// Builder for the "Update current user" endpoint (<c>POST /api/users/@me</c>).
	///
	/// Every member is optional: an empty string means "no change",
	/// <c>null</c> means "remove the stored value", and any other value applies the change.
	/// Collections follow the same rule: an empty array means "no change",
	/// <c>null</c> clears the list.
	/// </summary>
	/// <example>
	/// <code>
	/// await Main.UserAPI.UpdateCurrent(new UpdateCurrentRequest {
	///     Avatar = identifier.ToString()
	/// });
	/// </code>
	/// </example>
	public class UpdateCurrentRequest : IUpdateCurrentUserRequest {
		/// <inheritdoc />
		public string Username { get; set; } = string.Empty;

		/// <inheritdoc />
		public string Display { get; set; } = string.Empty;

		/// <inheritdoc />
		public string Bio { get; set; } = string.Empty;

		/// <inheritdoc />
		public string Pronoun { get; set; } = string.Empty;

		/// <inheritdoc />
		public string Email { get; set; } = string.Empty;

		/// <inheritdoc />
		public string CurrentPassword { get; set; } = string.Empty;

		/// <inheritdoc />
		public string Password { get; set; } = string.Empty;

		/// <inheritdoc />
		public string TwofaToken { get; set; } = string.Empty;

		/// <inheritdoc />
		public ILinkEntry[] Links { get; set; } = Array.Empty<ILinkEntry>();

		/// <inheritdoc />
		public string[] Tags { get; set; } = Array.Empty<string>();

		/// <inheritdoc />
		public string Thumbnail { get; set; } = string.Empty;

		/// <inheritdoc />
		public string Banner { get; set; } = string.Empty;

		/// <inheritdoc />
		public string Home { get; set; } = string.Empty;

		/// <inheritdoc />
		public string Avatar { get; set; } = string.Empty;

		/// <inheritdoc />
		public string Presence { get; set; } = string.Empty;

		/// <inheritdoc />
		public string PresenceStatus { get; set; } = string.Empty;

		public JObject ToJson() {
			var obj = new JObject();

			// Identity and credentials: only sent when explicitly set.
			if (Username is { Length: > 0 })
				obj["username"] = JValue.CreateString(Username);

			// Password change: the current password is required alongside the new one.
			if (Password is { Length: > 0 }) {
				obj["password"] = JValue.CreateString(Password);
				if (CurrentPassword is { Length: > 0 })
					obj["current_password"] = JValue.CreateString(CurrentPassword);
			}

			// TOTP two-factor code is named factor_code in the API.
			if (TwofaToken is { Length: > 0 })
				obj["factor_code"] = JValue.CreateString(TwofaToken);

			// Presence status: oja, ojf, online, busy, dnd, stream, offline
			if (Presence is { Length: > 0 })
				obj["presence"] = JValue.CreateString(Presence);

			// Nullable fields: empty = no change, null = remove, other = set
			SetNullable(obj, "display", Display);
			SetNullable(obj, "bio", Bio);
			SetNullable(obj, "pronoun", Pronoun);
			SetNullable(obj, "email", Email);
			SetNullable(obj, "thumbnail", Thumbnail);
			SetNullable(obj, "banner", Banner);
			SetNullable(obj, "home", Home);
			SetNullable(obj, "avatar", Avatar);
			SetNullable(obj, "presence_status", PresenceStatus);

			// Collections: empty = no change, null = clear, other = set
			if (Links == null)
				obj["links"] = JValue.CreateNull();
			else if (Links.Length > 0)
				obj["links"] = new JArray(Links
					.Where(link => link != null)
					.Select(link => (object)new JObject {
						["label"] = JValue.CreateString(link.Label ?? string.Empty),
						["value"] = JValue.CreateString(link.Value ?? string.Empty)
					})
					.ToArray());

			if (Tags == null)
				obj["tags"] = JValue.CreateNull();
			else if (Tags.Length > 0)
				obj["tags"] = new JArray(Tags.Select(tag => (object)tag).ToArray());

			return obj;
		}

		private static void SetNullable(JObject obj, string key, string value) {
			if (value == null)
				obj[key] = JValue.CreateNull();
			else if (value.Length > 0)
				obj[key] = JValue.CreateString(value);
		}
	}
}