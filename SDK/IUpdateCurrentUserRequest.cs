namespace Nox.Users {
	public interface IUpdateCurrentUserRequest {
		public IUpdateCurrentUserRequest SetUsername(string username);

		public IUpdateCurrentUserRequest SetDisplay(string display);

		public IUpdateCurrentUserRequest SetEmail(string email);

		public IUpdateCurrentUserRequest SetPassword(string password);

		public IUpdateCurrentUserRequest SetTwofaToken(string twofaToken);

		public IUpdateCurrentUserRequest SetBio(string bio);

		public IUpdateCurrentUserRequest SetThumbnail(string thumbnail);

		public IUpdateCurrentUserRequest SetBanner(string banner);

		public IUpdateCurrentUserRequest SetLinks(string[] links);

		public IUpdateCurrentUserRequest SetHome(string home);

		public IUpdateCurrentUserRequest SetTags(string[] tags);

		public IUpdateCurrentUserRequest SetAvatar(string avatar);

		public string GetUsername();

		public string GetDisplay();

		public string GetEmail();

		public string GetPassword();

		public string GetTwofaToken();

		public string GetBio();

		public string GetThumbnail();

		public string GetBanner();

		public string[] GetLinks();

		public string GetHome();

		public string GetAvatar();

		public string[] GetTags();
	}
}