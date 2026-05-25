namespace Nox.Users {
	/// <summary>
	/// Represents the presence information of a user,
	/// including their current status and an optional text message.
	/// </summary>
	public interface IUserPresence {
		/// <summary>
		/// Gets the current status of the user.
		/// </summary>
		public UserStatus Status { get; }

		/// <summary>
		/// Gets an optional text message associated with the user's presence.
		/// Can be null.
		/// </summary>
		public string Text { get; }
	}
}