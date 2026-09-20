using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Nox.Users;

namespace Nox.Users.Runtime.Networks {
	public class UpdateCurrentUserRequest : IUpdateCurrentUserRequest {
		public string Username { get; set; } = string.Empty;
		public string Display { get; set; } = string.Empty;
		public string Bio { get; set; } = string.Empty;
		public string Pronoun { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string CurrentPassword { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public string TwofaToken { get; set; } = string.Empty;
		public ILinkEntry[] Links { get; set; } = Array.Empty<ILinkEntry>();
		public string[] Tags { get; set; } = Array.Empty<string>();
		public string Thumbnail { get; set; } = string.Empty;
		public string Banner { get; set; } = string.Empty;
		public string Home { get; set; } = string.Empty;
		public string Avatar { get; set; } = string.Empty;
		public string Presence { get; set; } = string.Empty;
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

		public static UpdateCurrentUserRequest FromBase(IUpdateCurrentUserRequest request)
			=> new() {
				Username        = request.Username,
				Display         = request.Display,
				Bio             = request.Bio,
				Pronoun         = request.Pronoun,
				Email           = request.Email,
				CurrentPassword = request.CurrentPassword,
				Password        = request.Password,
				TwofaToken      = request.TwofaToken,
				Links           = request.Links,
				Tags            = request.Tags,
				Thumbnail       = request.Thumbnail,
				Banner          = request.Banner,
				Home            = request.Home,
				Avatar          = request.Avatar,
				Presence        = request.Presence,
				PresenceStatus  = request.PresenceStatus
			};
	}
}