namespace Nox.Users {
	/// <summary>
	/// Represents the relationship between a user and the current authenticated user,
	/// providing information about how the user is related to the current user and vice versa.
	/// </summary>
	public interface IUserRelation {
		/// <summary>
		/// Gets the relationship type of the user with the current user.
		/// <see cref="UserRelationType.NONE"/> when there is no relationship.
		/// </summary>
		public UserRelationType In { get; }

		/// <summary>
		/// Gets the relationship type of the current user with the user.
		/// <see cref="UserRelationType.NONE"/> when there is no relationship.
		/// </summary>
		public UserRelationType Out { get; }
	}
}