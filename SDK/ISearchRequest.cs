using Nox.CCK.Utils;
namespace Nox.Users {
	/// <summary>
	/// Represents a search request for users.
	/// </summary>
	public interface ISearchRequest {
		/// <summary>
		/// Search query.
		/// Empty means no textual filter.
		/// </summary>
		public string Query { get; set; }

		/// <summary>
		/// Identifiers to filter the search results.
		/// Empty means no identifier filter.
		/// </summary>
		public Identifier[] Ids { get; set; }

		/// <summary>
		/// Offset for the search results.
		/// Zero means no offset.
		/// </summary>
		public uint Offset { get; set; }

		/// <summary>
		/// Limit for the search results.
		/// Zero means the server default.
		/// </summary>
		public uint Limit { get; set; }
	}
}