namespace Nox.Users {
	/// <summary>
	/// Represents a request to update the profile of the authenticated user
	/// (<c>POST /api/users/@me</c>).
	///
	/// Every member is optional. For nullable members an empty value means
	/// "no change", <c>null</c> means "remove the stored value",
	/// and any other value applies the change.
	/// </summary>
	public interface IUpdateCurrentUserRequest {
		/// <summary>
		/// New unique username (lowercase letters, numbers, dots, hyphens and underscores).
		/// Empty means no change.
		/// </summary>
		public string Username { get; set; }

		/// <summary>
		/// Display name.
		/// Empty means no change, <c>null</c> removes it.
		/// </summary>
		public string Display { get; set; }

		/// <summary>
		/// Biography text.
		/// Empty means no change, <c>null</c> removes it.
		/// </summary>
		public string Bio { get; set; }

		/// <summary>
		/// Pronouns.
		/// Empty means no change, <c>null</c> removes them.
		/// </summary>
		public string Pronoun { get; set; }

		/// <summary>
		/// New email address.
		/// Empty means no change, <c>null</c> removes it.
		/// </summary>
		public string Email { get; set; }

		/// <summary>
		/// Current password, required when <see cref="Password"/> is set.
		/// </summary>
		public string CurrentPassword { get; set; }

		/// <summary>
		/// New password (6 to 128 characters), requires <see cref="CurrentPassword"/>.
		/// Empty means no change.
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// TOTP two-factor code (6 digits), sent as <c>factor_code</c>.
		/// Required for sensitive operations.
		/// Empty means no change.
		/// </summary>
		public string TwofaToken { get; set; }

		/// <summary>
		/// External links list.
		/// Empty means no change, <c>null</c> clears it.
		/// </summary>
		public ILinkEntry[] Links { get; set; }

		/// <summary>
		/// Tags list (<c>usr:*</c> format).
		/// Empty means no change, <c>null</c> clears it.
		/// </summary>
		public string[] Tags { get; set; }

		/// <summary>
		/// Thumbnail URL.
		/// Empty means no change, <c>null</c> removes it.
		/// </summary>
		public string Thumbnail { get; set; }

		/// <summary>
		/// Banner URL.
		/// Empty means no change, <c>null</c> removes it.
		/// </summary>
		public string Banner { get; set; }

		/// <summary>
		/// Home world identifier.
		/// Empty means no change, <c>null</c> removes it.
		/// </summary>
		public string Home { get; set; }

		/// <summary>
		/// Active avatar identifier.
		/// Empty means no change, <c>null</c> removes it.
		/// </summary>
		public string Avatar { get; set; }

		/// <summary>
		/// Presence status, one of <c>oja</c>, <c>ojf</c>, <c>online</c>,
		/// <c>busy</c>, <c>dnd</c>, <c>stream</c> or <c>offline</c>.
		/// Empty means no change.
		/// </summary>
		public string Presence { get; set; }

		/// <summary>
		/// Custom presence status text.
		/// Empty means no change, <c>null</c> removes it.
		/// </summary>
		public string PresenceStatus { get; set; }
	}
}