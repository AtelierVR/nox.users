namespace Nox.Users {
	public interface IAuthToken {
		public string GetToken();
		public bool   IsIntegrity();
		public string ToHeader();
	}
}