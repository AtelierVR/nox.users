using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Nox.Users;

namespace Nox.Users.Runtime.Networks {
	public class UpdateCurrentUserRequest : IUpdateCurrentUserRequest {
		private string   _username   = string.Empty;
		private string   _display    = string.Empty;
		private string   _email      = string.Empty;
		private string   _password   = string.Empty;
		private string   _twofaToken = string.Empty;
		private string   _bio        = string.Empty;
		private string   _thumbnail  = string.Empty;
		private string   _banner     = string.Empty;
		private string[] _links      = Array.Empty<string>();
		private string   _home       = string.Empty;
		private string   _avatar     = string.Empty;
		private string[] _tags       = Array.Empty<string>();

		public IUpdateCurrentUserRequest SetUsername(string username) {
			_username = username;
			return this;
		}

		public IUpdateCurrentUserRequest SetDisplay(string display) {
			_display = display;
			return this;
		}

		public IUpdateCurrentUserRequest SetEmail(string email) {
			_email = email;
			return this;
		}

		public IUpdateCurrentUserRequest SetPassword(string password) {
			_password = password;
			return this;
		}

		public IUpdateCurrentUserRequest SetTwofaToken(string twofaToken) {
			_twofaToken = twofaToken;
			return this;
		}

		public IUpdateCurrentUserRequest SetBio(string bio) {
			_bio = bio;
			return this;
		}

		public IUpdateCurrentUserRequest SetThumbnail(string thumbnail) {
			_thumbnail = thumbnail;
			return this;
		}

		public IUpdateCurrentUserRequest SetBanner(string banner) {
			_banner = banner;
			return this;
		}

		public IUpdateCurrentUserRequest SetLinks(string[] links) {
			_links = links ?? Array.Empty<string>();
			return this;
		}

		public IUpdateCurrentUserRequest SetHome(string home) {
			_home = home;
			return this;
		}

		public IUpdateCurrentUserRequest SetTags(string[] tags) {
			_tags = tags ?? Array.Empty<string>();
			return this;
		}

		public IUpdateCurrentUserRequest SetAvatar(string avatar) {
			_avatar = avatar;
			return this;
		}

		public string GetUsername()
			=> _username;

		public string GetDisplay()
			=> _display;

		public string GetEmail()
			=> _email;

		public string GetPassword()
			=> _password;

		public string GetTwofaToken()
			=> _twofaToken;

		public string GetBio()
			=> _bio;

		public string GetThumbnail()
			=> _thumbnail;

		public string GetBanner()
			=> _banner;

		public string[] GetLinks()
			=> _links ?? Array.Empty<string>();

		public string GetHome()
			=> _home;

		public string GetAvatar()
			=> _avatar;

		public string[] GetTags()
			=> _tags ?? Array.Empty<string>();

		public JObject ToJson() {
			var obj = new JObject();

			if (_username is { Length: > 0 })
				obj["username"] = JValue.CreateString(_username);

			if (_display == null)
				obj["display"] = JValue.CreateNull();
			else if (_display.Length > 0)
				obj["display"] = JValue.CreateString(_display);

			if (_bio == null)
				obj["bio"] = JValue.CreateNull();
			else if (_bio.Length > 0)
				obj["bio"] = JValue.CreateString(_bio);

			if (_banner == null)
				obj["banner"] = JValue.CreateNull();
			else if (_banner.Length > 0)
				obj["banner"] = JValue.CreateString(_banner);

			if (_thumbnail == null)
				obj["thumbnail"] = JValue.CreateNull();
			else if (_thumbnail.Length > 0)
				obj["thumbnail"] = JValue.CreateString(_thumbnail);

			if (_email == null)
				obj["email"] = JValue.CreateNull();
			else if (_email.Length > 0)
				obj["email"] = JValue.CreateString(_email);

			if (_password is { Length: > 0 }) {
				obj["password"]         = JValue.CreateString(_password);
				obj["current_password"] = JValue.CreateString(_password);
			}

			if (_twofaToken is { Length: > 0 })
				obj["twofa_token"] = JValue.CreateString(_twofaToken);

			if (_links is { Length: > 0 })
				obj["links"] = new JArray(_links.ToArray<object>());

			if (_home == null)
				obj["home"] = JValue.CreateNull();
			else if (_home is { Length: > 0 })
				obj["home"] = JValue.CreateString(_home);

			if (_avatar == null)
				obj["avatar"] = JValue.CreateNull();
			else if (_avatar is { Length: > 0 })
				obj["avatar"] = JValue.CreateString(_avatar);

			if (_tags is { Length: > 0 })
				obj["tags"] = new JArray(_tags.ToArray<object>());

			return obj;
		}

		public static UpdateCurrentUserRequest FromBase(IUpdateCurrentUserRequest request)
			=> new() {
				_username   = request.GetUsername(),
				_display    = request.GetDisplay(),
				_email      = request.GetEmail(),
				_password   = request.GetPassword(),
				_twofaToken = request.GetTwofaToken(),
				_bio        = request.GetBio(),
				_thumbnail  = request.GetThumbnail(),
				_banner     = request.GetBanner(),
				_links      = request.GetLinks(),
				_home       = request.GetHome(),
				_tags       = request.GetTags()
			};
	}
}