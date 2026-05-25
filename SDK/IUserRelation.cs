namespace Nox.Users {
	/// <summary>
	/// Represents the relationship between a user and the current authenticated user,
	/// providing information about how the user is related to the current user and vice versa.
	/// </summary>
	public interface IUserRelation {
		/// <summary>
		/// Gets the relationship status of the user with the current user.
		/// </summary>
		public string In { get; }

		/// <summary>
		/// Gets the relationship status of the current user with the user.
		/// </summary>
		public string Out { get; }
	}
}