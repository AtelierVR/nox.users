namespace Nox.Users {
	/// <summary>
	/// Presence update for the current user (<c>POST /api/users/@me</c>).
	/// Carries the presence status along with its optional custom text.
	/// </summary>
	public interface IUpdatePresence {
		/// <summary>
		/// Presence status to apply, one of <c>event</c>, <c>public</c>, <c>online</c>,
		/// <c>busy</c>, <c>dnd</c>, <c>stream</c> or <c>offline</c>.
		/// <c>null</c> means no change.
		/// </summary>
		public UserStatus Status { get; set; }

		/// <summary>
		/// Custom presence status text.
		/// Empty means no change, <c>null</c> removes it.
		/// </summary>
		public string Text { get; set; }
	}

	/// <summary>
	/// Default mutable <see cref="IUpdatePresence"/> implementation.
	/// </summary>
	public class UpdatePresence : IUpdatePresence {
		/// <inheritdoc />
		public UserStatus Status { get; set; } = UserStatus.NONE;

		/// <inheritdoc />
		public string Text { get; set; } = string.Empty;
	}
}
