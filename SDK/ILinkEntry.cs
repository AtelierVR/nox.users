namespace Nox.Users {
	/// <summary>
	/// Represents a link entry associated with a user,
	/// containing a label and a URL value.
	/// </summary>
	public interface ILinkEntry {
		/// <summary>
		/// Gets the label of the link entry,
		/// which is a human-readable name for the link.
		/// </summary>
		public string Label { get; }

		/// <summary>
		/// Gets the URL value of the link entry,
		/// which is the actual link that can be visited.
		/// </summary>
		public string Value { get; }
	}
}