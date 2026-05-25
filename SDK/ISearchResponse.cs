using Cysharp.Threading.Tasks;

namespace Nox.Users {
	/// <summary>
	/// Represents the response of a search query for users.
	/// </summary>
	public interface ISearchResponse {
		/// <summary>
		/// Gets the array of users returned by the search query.
		/// </summary>
		public IUser[] Items { get; }

		/// <summary>
		/// Gets the total number of users matching the search query, regardless of pagination.
		/// </summary>
		public uint Total { get; }

		/// <summary>
		/// Gets the maximum number of users returned in this response (pagination limit).
		/// </summary>
		public uint Limit { get; }

		/// <summary>
		/// Gets the offset of the first user in this response relative to the total number of matching users (pagination offset).
		/// </summary>
		public uint Offset { get; }

		/// <summary>
		/// Checks if there are more users available after the current page of results.
		/// </summary>
		/// <returns></returns>
		public bool HasNext();

		/// <summary>
		/// Checks if there are previous users available before the current page of results.
		/// </summary>
		/// <returns></returns>
		public bool HasPrevious();

		/// <summary>
		/// Asynchronously retrieves the next page of search results, if available.
		/// </summary>
		/// <returns></returns>
		public UniTask<ISearchResponse> Next();

		/// <summary>
		/// Asynchronously retrieves the previous page of search results, if available.
		/// </summary>
		/// <returns></returns>
		public UniTask<ISearchResponse> Previous();
	}
}